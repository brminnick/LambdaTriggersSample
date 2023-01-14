using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;

namespace LambdaTriggers;

public class SQSLambdaFunction
{
	public static string FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
	{
		Console.WriteLine($"Beginning to process {sqsEvent.Records.Count} records...");

		foreach (var record in sqsEvent.Records)
		{
			Console.WriteLine($"Message ID: {record.MessageId}");
			Console.WriteLine($"Event Source: {record.EventSource}");

			Console.WriteLine($"Record Body:");
			Console.WriteLine(record.Body);
		}

		Console.WriteLine("Processing complete.");

		return $"Processed {sqsEvent.Records.Count} records.";
	}

	static Task Main(string[] args) =>
	LambdaBootstrapBuilder.Create((SQSEvent sqsEvent, ILambdaContext context) => FunctionHandler(sqsEvent, context), new DefaultLambdaJsonSerializer())
							.Build()
							.RunAsync();
}
