using System;
using Fnproject.Fn.Fdk;
using Newtonsoft.Json.Linq;
using Oci.ObjectstorageService;
using Oci.Common.Auth;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Oci.ObjectstorageService.Requests;


[assembly:InternalsVisibleTo("Function.Tests")]
namespace Function 
{
	class Greeter 
	{
		public async Task<string> greet(string input) 
		{
			if (input.StartsWith('{'))
			{
				dynamic sourceData = JObject.Parse (input);
				Console.WriteLine ("Bucket = " + sourceData.data.additionalDetails.bucketName);
				Console.WriteLine ("Object = " + sourceData.data.resourceName);
				Console.WriteLine ("Resource ID = " + sourceData.data.resourceId);
				try
				{
					
					var resourcePrincipalsProvider = ResourcePrincipalAuthenticationDetailsProvider.GetProvider();

					var objectClient = new Oci.ObjectstorageService.ObjectStorageClient (resourcePrincipalsProvider);
					System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken ();

					Console.WriteLine ("Namespace = " + sourceData.data.additionalDetails["namespace"]);

					var request = new Oci.ObjectstorageService.Requests.GetObjectRequest ()
					{
						BucketName = sourceData.data.additionalDetails.bucketName,
						ObjectName = sourceData.data.resourceName,
						NamespaceName = sourceData.data.additionalDetails["namespace"],
					};

					var getResponce = await objectClient.GetObject (request, null, cancellationToken, System.Net.Http.HttpCompletionOption.ResponseContentRead);
					if (getResponce != null)
					{
						string[] fileNameParts = sourceData.data.resourceName.ToString().Split("/");
						var putObjectRequest = new Oci.ObjectstorageService.Requests.PutObjectRequest ()
						{
							BucketName = sourceData.data.additionalDetails.bucketName,
							NamespaceName = sourceData.data.additionalDetails["namespace"],
							ObjectName = "thumbnail-" + fileNameParts[fileNameParts.Length -1],
							PutObjectBody = new MemoryStream (),
						};

						getResponce.InputStream.CopyTo (putObjectRequest.PutObjectBody);
						putObjectRequest.PutObjectBody.Flush();
						putObjectRequest.PutObjectBody.Position = 0;

						await objectClient.PutObject (putObjectRequest);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine ("Error getting object " + ex.Message);
				}
			}

			return string.Format("Hello {0}!",
				string.IsNullOrEmpty(input) ? "World" : input.Trim());
		}

		static void Main(string[] args) { Fdk.Handle(args[0]); }
	}
}
