using CloudFoundry.CloudController.V2.Client.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudFoundry.VisualStudio.ProjectPush
{
    public class ServiceProfile
    {
        public ListAllServiceInstancesForSpaceResponse ServiceInstance { get; set; }

        public RetrieveServicePlanResponse ServicePlan { get; set; }

        public RetrieveServiceResponse Service { get; set; }

        public bool Used { get; set; }
    }
}
