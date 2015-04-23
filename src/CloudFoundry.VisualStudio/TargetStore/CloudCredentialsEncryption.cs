using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace CloudFoundry.VisualStudio.TargetStore
{
    public class CloudCredentialsEncryption
    {
        private enum Store
        {
            /// <summary>
            /// User independent store. Less secure but works well from ASP.Net
            /// </summary>
            MachineStore = 1,
            /// <summary>
            /// User specific store. Machine and user affinity.
            /// </summary>
            UserStore
        };

        #region DLL Imports

        /// <summary>
        /// Import of CryptoAPIs CryptProtectData. Parameters ripped from the SDK.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), 
        DllImport("Crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern bool CryptProtectData(
            ref DATA_BLOB pDataIn,
            String szDataDescr,
            ref DATA_BLOB pOptionalEntropy,
            IntPtr pvReserved,
            ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct,
            int dwFlags,
            ref DATA_BLOB pDataOut);

        /// <summary>
        /// Import of CryptoAPIs CryptUnprotectData. Parameters ripped from the SDK.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"),
        DllImport("Crypt32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern bool CryptUnprotectData(
            ref DATA_BLOB pDataIn,
            String szDataDescr,
            ref DATA_BLOB pOptionalEntropy,
            IntPtr pvReserved,
            ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct,
            int dwFlags,
            ref DATA_BLOB pDataOut);


        #endregion

        #region Structure Definitions and Constraints
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DATA_BLOB : IDisposable
        {
            public int cbData;
            public IntPtr pbData;

            public void Dispose()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct CRYPTPROTECT_PROMPTSTRUCT
        {
            public int cbSize;
            public int dwPromptFlags;
            public IntPtr hwndApp;
            public String szPrompt;
        }

        private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
        private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;
        #endregion

        #region Constructor
        private Store store;

        /// <summary>
        /// Initializes a new instance of <see cref='Dpapi.DataProtector'/> from a type of <see cref='Dpapi.Store'/>.
        /// </summary>
        /// <param name="store">A <see cref="Dpapi.Store"/> to initialize the DataProtector.</param>
        public CloudCredentialsEncryption()
        {
            this.store = Store.UserStore;
        }
        #endregion



        public static SecureString GetSecureString(string text)
        {
            SecureString result = new SecureString();
            try
            {
                foreach (char ch in text)
                {
                    result.AppendChar(ch);
                }

                return result;
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
                }
                throw;
            }
        }

        public static string GetUnsecureString(SecureString securePassword)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public string Encrypt(SecureString plainText)
        {
            byte[] cipherText = Encrypt(Encoding.Unicode.GetBytes(GetUnsecureString(plainText)), new byte[0]);
            return Convert.ToBase64String(cipherText);
        }

        public SecureString Decrypt(string encryptedText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] plainText = Decrypt(cipherTextBytes, new byte[0]);
            return GetSecureString(Encoding.Unicode.GetString(plainText));
        }

        #region Encrypt and Decrypt


        /// <summary>
        /// Encrypts an array of bytes with an entropy value for added security and returns the cipher text.
        /// </summary>
        /// <param name="plainText">Array of bytes to be encrypted.</param>
        /// <param name="entropyText">Optional Entropy value for further protection when using the machine store.</param>
        /// <returns>Array of bytes with the cipher text needed later with <see cref="Dpapi.DataProtector.Decrypt"/></returns>
        /// <example>This example shows how to encrypt text.
        /// <code>
        ///	DataProtector dp = new DataProtector(DataProtector.Store.MachineStore);
        ///	byte[] dataToEncrypt = Encoding.Unicode.GetBytes("Secret user name, password, and connection string!");
        /// CipherText.Text = Convert.ToBase64String(dp.Encrypt(dataToEncrypt, "entropy text"));
        /// </code>
        /// </example>
        private byte[] Encrypt(byte[] plainText, byte[] entropyText)
        {
            bool retVal = false;

            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB cipherTextBlob = new DATA_BLOB();
            DATA_BLOB entropyBlob = new DATA_BLOB();

            CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
            InitPromptstruct(ref prompt);

            int dwFlags;
            try
            {
                try
                {
                    int bytesSize = plainText.Length;
                    plainTextBlob.pbData = Marshal.AllocHGlobal(bytesSize);
                    if (plainTextBlob.pbData == IntPtr.Zero)
                    {
                        throw new InvalidOperationException("Unable to allocate memory for buffer.");
                    }
                    plainTextBlob.cbData = bytesSize;
                    Marshal.Copy(plainText, 0, plainTextBlob.pbData, bytesSize);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Exception marshalling data.", ex);
                }

                if (store == Store.MachineStore)
                {
                    // MachineStore
                    dwFlags = CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN;

                    try
                    {
                        int bytesSize = entropyText.Length;
                        entropyBlob.pbData = Marshal.AllocHGlobal(bytesSize);
                        if (entropyBlob.pbData == IntPtr.Zero)
                        {
                            throw new InvalidOperationException("Unable to allocate entropy data buffer.");
                        }
                        Marshal.Copy(entropyText, 0, entropyBlob.pbData, bytesSize);
                        entropyBlob.cbData = bytesSize;
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Exception entropy marshalling data. ", ex);
                    }
                }
                else
                {
                    // UserStore
                    dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
                }
                retVal = CryptProtectData(ref plainTextBlob, null, ref entropyBlob, IntPtr.Zero, ref prompt, dwFlags, ref cipherTextBlob);
                if (!retVal)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Exception encrypting.", ex);
            }
            byte[] cipherText = new byte[cipherTextBlob.cbData];
            Marshal.Copy(cipherTextBlob.pbData, cipherText, 0, cipherTextBlob.cbData);
            return cipherText;
        }

   
        /// <summary>
        /// Decrypts an array of bytes previously encrypted along with an entropy value.
        /// </summary>
        /// <param name="cipherText">Cipher text from previous encryption.</param>
        /// <param name="entropyText">Optional Entropy value for further protection when using the machine store.</param>
        /// <returns>Array of bytes containing the decrypted data.</returns>
        /// <example>This exmaple shows how to decrypt text.
        /// <code>
        ///	DataProtector dp = new DataProtector(DataProtector.Store.MachineStore);
        /// byte[] dataToDecrypt = Convert.FromBase64String(CipherText.Text);
        /// DecryptResults.Text = Encoding.Unicode.GetString(dp.Decrypt(dataToDecrypt, "entropy text"));
        /// </code>
        /// </example>
        private byte[] Decrypt(byte[] cipherText, byte[] entropyText)
        {
            bool retVal = false;
            DATA_BLOB plainTextBlob = new DATA_BLOB();
            DATA_BLOB cipherBlob = new DATA_BLOB();
            CRYPTPROTECT_PROMPTSTRUCT prompt = new CRYPTPROTECT_PROMPTSTRUCT();
            InitPromptstruct(ref prompt);
            try
            {
                try
                {
                    int cipherTextSize = cipherText.Length;
                    cipherBlob.pbData = Marshal.AllocHGlobal(cipherTextSize);
                    if (cipherBlob.pbData == IntPtr.Zero)
                    {
                        throw new InvalidOperationException("Unable to allocate cipherText buffer.");
                    }
                    cipherBlob.cbData = cipherTextSize;
                    Marshal.Copy(cipherText, 0, cipherBlob.pbData, cipherBlob.cbData);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Exception marshalling data.", ex);
                }
                DATA_BLOB entropyBlob = new DATA_BLOB();
                int dwFlags;

                if (store == Store.MachineStore)
                {
                    // MachineStore
                    dwFlags = CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN;

                    try
                    {
                        int bytesSize = entropyText.Length;
                        entropyBlob.pbData = Marshal.AllocHGlobal(bytesSize);
                        if (entropyBlob.pbData == IntPtr.Zero)
                        {
                            throw new InvalidOperationException("Unable to allocate entropy buffer.");
                        }
                        entropyBlob.cbData = bytesSize;
                        Marshal.Copy(entropyText, 0, entropyBlob.pbData, bytesSize);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("Exception entropy marshalling data. ", ex);
                    }
                }
                else
                {
                    // UserStore
                    dwFlags = CRYPTPROTECT_UI_FORBIDDEN;
                }
                retVal = CryptUnprotectData(ref cipherBlob, null, ref entropyBlob, IntPtr.Zero, ref prompt, dwFlags, ref plainTextBlob);
                if (!retVal)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                //Free the blob and entropy.
                if (cipherBlob.pbData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(cipherBlob.pbData);
                }
                if (entropyBlob.pbData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(entropyBlob.pbData);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Exception decrypting. ", ex);
            }
            byte[] plainText = new byte[plainTextBlob.cbData];
            Marshal.Copy(plainTextBlob.pbData, plainText, 0, plainTextBlob.cbData);
            return plainText;
        }
        #endregion

        #region Helper Functions
        private static void InitPromptstruct(ref CRYPTPROTECT_PROMPTSTRUCT ps)
        {
            ps.cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT));
            ps.dwPromptFlags = 0;
            ps.hwndApp = IntPtr.Zero;
            ps.szPrompt = null;
        }
        #endregion

    }
}
