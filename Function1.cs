
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Model;
using Azure.Storage.Blobs;
using System.Text;
using SigmaProConnectFileValidation.Data;
using Microsoft.EntityFrameworkCore;


namespace SigmaProConnectFileValidation
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;
        private const string ContainerName = "sigmahl7";


        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
          
        }

        [Function(nameof(Function1))]
        public async System.Threading.Tasks.Task Run([BlobTrigger(ContainerName + "/new/{name}", Connection = "")] Stream myBlob, string name, FunctionContext context)
        {
            using var blobStreamReader = new StreamReader(myBlob);
            var content = await blobStreamReader.ReadToEndAsync();
            var log = context.GetLogger<Function1>();
            log.LogInformation($"Processing blob: {name}");
            
                try
                {
                    // Reset the stream position to the beginning
                    myBlob.Position = 0;

                    using (var reader = new StreamReader(myBlob, Encoding.UTF8))
                    {
                        var hl7Data = await reader.ReadToEndAsync();
                        if (!string.IsNullOrEmpty(hl7Data))
                        {
                            if (ValidateBlob(hl7Data, name, log))
                            {

                                var resources = ParseHL7(hl7Data, log);

                            }
                            else
                            {
                               log.LogWarning("Blob validation failed.");
                            }
                        }
                        else
                        {
                            log.LogWarning("Empty or null HL7 data.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"Error reading or processing blob content: {ex.Message}");
                }
            
        }
        private SigmaproConnectContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SigmaproConnectContext>();
            optionsBuilder.UseNpgsql("Host=sigmaprodb.postgres.database.azure.com,5432;Database=sigmapro_iis;Username=sigmaprodb_user;Password=Rules@23$$11;TrustServerCertificate=False"); // Replace "YourConnectionString" with your PostgreSQL connection string
            return new SigmaproConnectContext(optionsBuilder.Options);
        }

        public bool ValidateBlob(string stream, string fileName, ILogger logger)
        {
            try
            {
                string fileExtension = Path.GetExtension(fileName);
                if (fileExtension != ".txt" && fileExtension != ".hl7")
                {
                    logger.LogError($"Validation failed: Unsupported file type '{fileExtension}'.");
                    return false;
                }

               

                if (fileExtension == ".hl7" || fileExtension == ".txt")
                {
                    
                        return true;
                   
                }
                else
                {
                    logger.LogError($"Validation failed: Unsupported file type '{fileExtension}'.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error during validation: {ex.Message}");
                return false;
            }
        }

        public List<Resource> ParseHL7(string hl7Data, ILogger logger)
        {
            List<Resource> resources = new List<Resource>();

            // Split the HL7 message into segments
            string[] segments = hl7Data.Split('\n');

            foreach (string segment in segments)
            {
                string[] fields = segment.Split('|');
                string segmentType = fields[0];

                switch (segmentType)
                {
                    case "MSH":
                        // Process MSH segment
                        ProcessMSHSegment(fields, logger);
                        break;
                    case "PID":
                        // Process PID segment
                        resources.Add(ParsePID(fields));
                        break;
                    case "NK1":
                        // Process NK1 segment
                        resources.Add(ParseNK1(fields));
                        break;
                    case "IN1":
                        // Process IN1 segment
                        resources.Add(ParseIN1(fields));
                        break;
                    case "RXA":
                        // Process RXA segment for vaccine administration
                        resources.Add(ParseRXA(fields));
                        break;
                    case "RXR":
                        // Process RXR segment for vaccine route
                        resources.Add(ParseRXR(fields));
                        break;
                    case "OBX":
                        // Process OBX segment for vaccine observations
                        resources.Add(ParseOBX(fields));
                        break;
                    case "PV1":
                        // Process PV1 segment
                        resources.Add(ParsePV1(fields));
                        break;
                    // Add cases for other segment types as needed
                    default:
                        // Handle unknown or unsupported segment types
                        // Log a warning or take appropriate action
                        break;
                }
            }

            return resources;
        }

        private void ProcessMSHSegment(string[] fields, ILogger logger)
        {
            if (fields.Length > 7)
            {
                string messageType = fields[8];
                string timestamp = fields[6];
                string sender = fields[2];
                string receiver = fields[4];

                Console.WriteLine($"Message Type: {messageType}");
                Console.WriteLine($"Timestamp: {timestamp}");
                Console.WriteLine($"Sender: {sender}");
                Console.WriteLine($"Receiver: {receiver}");
            }
        }

        private Encounter ParsePV1(string[] fields)
        {
            // Create a new Encounter resource
            Encounter encounter = new Encounter();

            // Extract relevant information from the PV1 segment fields and populate the resource
            encounter.Status = Encounter.EncounterStatus.Finished;
            encounter.Subject = new ResourceReference { Reference = $"Patient/{fields[1]}" }; // Assuming fields[1] contains the patient ID
            encounter.Period = new Period
            {
                Start = ParseDateTime(fields[44]), // Assuming fields[44] contains the start date/time of the encounter
                End = ParseDateTime(fields[45]) // Assuming fields[45] contains the end date/time of the encounter
            };
            // You may add more properties as needed

            return encounter;
        }

        private Observation ParseOBX(string[] fields)
        {
            // Create a new Observation resource
            Observation observation = new Observation();

            observation.Code = new CodeableConcept { Text = fields[3] };
            observation.Value = new Quantity
            {
                Value = Convert.ToDecimal(fields[5]),
                Unit = fields[6]
            };

            return observation;
        }

        private Patient ParsePID(string[] fields)
        {
            Patient patient = new Patient();
            patient.Id = fields[3];
            patient.Name.Add(new HumanName { Text = fields[5] });
            // Add more fields as needed

            return patient;
        }

        private Patient ParseNK1(string[] fields)
        {
            // Create a new Patient resource (assuming NK1 refers to another patient)
            Patient patient = new Patient();

            // Extract relevant information from NK1 segment and populate the patient resource
            patient.Id = fields[4];
            patient.Name.Add(new HumanName { Text = fields[1] });

            // Save patient data to the database
            SavePatientToDatabaseAsync(patient.Id, patient.Name.FirstOrDefault()?.Text).GetAwaiter().GetResult();

            // Add more fields as needed

            return patient;
        }

        private Claim ParseIN1(string[] fields)
        {
            // Create a new Claim resource
            Claim claim = new Claim();

            // Extract relevant information from IN1 segment and populate the claim resource
            claim.Id = fields[1];
            // Add more fields as needed

            return claim;
        }

        private MedicationAdministration ParseRXR(string[] fields)
        {
            // Create a new MedicationAdministration resource
            MedicationAdministration medicationAdministration = new MedicationAdministration();

            // Extract relevant information from the RXR segment fields and populate the resource
            medicationAdministration.Dosage = new MedicationAdministration.DosageComponent
            {
                Route = new CodeableConcept { Text = fields[1] }, // Assuming fields[1] contains the route of administration
                Site = new CodeableConcept { Text = fields[2] }   // Assuming fields[2] contains the administration site
                                                                  // You may add more properties as needed
            };
            // Populate other fields of the MedicationAdministration resource

            return medicationAdministration;
        }

        private MedicationAdministration ParseRXA(string[] fields)
        {
            // Create a new MedicationAdministration resource
            MedicationAdministration medicationAdministration = new MedicationAdministration();

            // Extract relevant information from the RXA segment fields and populate the resource
            medicationAdministration.Effective = new FhirDateTime(ParseDateTime(fields[1])); // Assuming fields[1] contains the administration date/time
            medicationAdministration.Medication = new ResourceReference { Reference = $"Medication/{fields[2]}" }; // Assuming fields[2] contains the medication code
            medicationAdministration.Dosage = new MedicationAdministration.DosageComponent
            {
                Route = new CodeableConcept { Text = fields[5] } // Assuming fields[5] contains the route of administration
                                                                 // Populate other dosage-related fields as needed
            };
            // Populate other fields of the MedicationAdministration resource

            return medicationAdministration;
        }

        // Helper method to parse date/time string to FHIR DateTime type
        private string ParseDateTime(string dateTimeString)
        {
            DateTime dateTime;
            if (DateTime.TryParse(dateTimeString, out dateTime))
            {
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ss"); // Format the date/time string according to FHIR DateTime format
            }
            // Handle parsing errors or invalid date/time formats
            throw new FormatException($"Invalid date/time format: {dateTimeString}");
        }

        private async Task<string> MoveBlob(string blobName, string destinationFolder)
        {
            try
            {
                // Read connection string from local.settings.json
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                // Initialize BlobServiceClient with the connection string
                var blobServiceClient = new BlobServiceClient(connectionString);

                // Get the blob container client
                var containerClient = blobServiceClient.GetBlobContainerClient("sigmahl7");

                // Get the blob client for the specified blob
                var blobClient = containerClient.GetBlobClient("new/" + blobName);

                // Get the URL of the blob
                var blobUrl = blobClient.Uri.AbsoluteUri;

                // Copy the blob to the destination folder
                var destinationBlobClient = containerClient.GetBlobClient(destinationFolder + "/" + blobName);
                await destinationBlobClient.StartCopyFromUriAsync(blobClient.Uri);

                // Delete the original blob
                await blobClient.DeleteIfExistsAsync();

                return blobUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving blob: {ex.Message}");
                return null;
            }
        }

        private async System.Threading.Tasks.Task SavePatientToDatabaseAsync(string patientId, string patientName)
        {

            using (var context = CreateDbContext())
            {
                var patientStage = new patient_stage
                {
                    PatientId = patientId,
                    PatientName = patientName
                };
                context.patient_stage.Add(patientStage);
                await context.SaveChangesAsync();
            }
        }
    }

}
