# migrate-lambda
This project focuses on migrating a Lmanda frunction running inside of AWS to OCI. The project retains the business logic, ImageProcessor.cs and rewrites the integration with AWS services to be OCI specific.

/Lambda/Thumbnail-Generator                     Origional Lambda Code
/OCI/thumbnailFunction                          OCI Function Code
/OCI/thumbnailFunction/doc/dynamic-group.txt    Definition for the OCI Dynamic Group
/OCI/thumbnailFunction/doc/policy.txt           IAM Policy to allow access to OCI storage buckets in your compartment

Note this code builds off the three part series on OCI functions that I piblished on BasementProgrammer.Com on getting your development environment set up for OCI Functions. If you are not already set up with the Fn Project you should review those blog posts first.

https://basementprogrammer.com/oracle-functions-and-net-getting-started-part-1
https://basementprogrammer.com/oracle-functions-and-net-getting-started-part-2
https://basementprogrammer.com/oracle-functions-and-net-getting-started-part-3


