# How to reproduce the error:

- Create cosmos service with database name = "Course"
- Create a new container with name = "ClassSessionContent" and PartitionKey  = "/partitionKey"
- Upload data from SampleData/ClassSessionContent.json" (only contains 10 data), you need to duplicate the sample data to a desirable amount
- Put your Cosmos service connection string to local.settings.json (field "CosmosDB")
- Run function ErrorDanny
