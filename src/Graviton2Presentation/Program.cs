using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Graviton2Presentation
{
    sealed class Program
    {
        private VpcStack _vpcStack;
        private PipelineStack _pipelineStack;
        private EksStack _eksStack;
        private App _app;
        public static void Main(string[] args)
        {
            string stackName = "GravitonID";
            Program program = CreateProgram(stackName);
            program.Synth();
        }

        private static Program CreateProgram(string stackName)
        {
            Program program = new Program();
            program._app = new App();
            program._vpcStack = new VpcStack(program._app, $"{stackName}-base", CreateStackProps());
            program._eksStack = new EksStack(program._app, $"{stackName}-eks", program._vpcStack.Vpc , CreateStackProps());
            program._pipelineStack = new PipelineStack(program._app, $"{stackName}-pipeline", CreateStackProps());
            return program;
        }

        private void Synth()
        {
            _app.Synth();
        }

        private static StackProps CreateStackProps()
        {
            return new StackProps
            {
                // If you don't specify 'env', this stack will be environment-agnostic.
                // Account/Region-dependent features and context lookups will not work,
                // but a single synthesized template can be deployed anywhere.

                // Uncomment the next block to specialize this stack for the AWS Account
                // and Region that are implied by the current CLI configuration.
                /*
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION"),
                }
                */

                // Uncomment the next block if you know exactly what Account and Region you
                // want to deploy the stack to.
                /*
                Env = new Amazon.CDK.Environment
                {
                    Account = "123456789012",
                    Region = "us-east-1",
                }
                */

                // For more information, see https://docs.aws.amazon.com/cdk/latest/guide/environments.html
            };
        }
    }
}
