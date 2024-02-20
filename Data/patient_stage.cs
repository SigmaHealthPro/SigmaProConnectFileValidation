using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigmaProConnectFileValidation.Data
{
    public class patient_stage
    {
        public string? PatientId { get; set; }

        public string? PatientName { get; set; }

        public Guid Id { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }
    }
}
