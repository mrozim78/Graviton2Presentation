using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace Graviton2Presentation
{
    public class PipelineStack : Stack
    {
        public CfnOutput CfnOutput { get; private set; }
        internal PipelineStack(Construct scope, string id, IStackProps props = null) : base(scope, id,
            props)
        {
            // The code that defines your stack goes here
            var name = "graviton2-pipeline-lab";
            var containerRepository = new Amazon.CDK.AWS.ECR.Repository(this, id: $"{name}-container",
                new Amazon.CDK.AWS.ECR.RepositoryProps
                {
                    RepositoryName = name
                });
            var codecommitRepo = new Amazon.CDK.AWS.CodeCommit.Repository(this, id: $"{name}-container-git",
                new Amazon.CDK.AWS.CodeCommit.RepositoryProps()
                {
                    RepositoryName = name,
                    Description = "Application code"
                    
                });
            var pipeline = new Amazon.CDK.AWS.CodePipeline.Pipeline(this, $"{name}-container--pipeline", new Amazon.CDK.AWS.CodePipeline.PipelineProps
            {
                PipelineName = name
                
            });

            var sourceOutput = new Amazon.CDK.AWS.CodePipeline.Artifact_();
            var dockerOutput86 = new Amazon.CDK.AWS.CodePipeline.Artifact_("x86_BuildOutput");
            var dockerOutputArm64 = new Amazon.CDK.AWS.CodePipeline.Artifact_("ARM64_BuildOutput");
            var manifestOutput = new Amazon.CDK.AWS.CodePipeline.Artifact_("ManifestOutput");
            
            var buildSpec86 = Amazon.CDK.AWS.CodeBuild.BuildSpec.FromSourceFilename("x86-buildspec.yml");
            var buildSpecArm64 = Amazon.CDK.AWS.CodeBuild.BuildSpec.FromSourceFilename("arm64-buildspec.yml");
            var buildSpecManifest = Amazon.CDK.AWS.CodeBuild.BuildSpec.FromSourceFilename("manifest-buildspec.yml");

            var dockerBuild86 = new  Amazon.CDK.AWS.CodeBuild.PipelineProject(this, "DockerBuild_x86", new  Amazon.CDK.AWS.CodeBuild.PipelineProjectProps
            {
                BuildSpec = buildSpec86,
                Environment = new BuildEnvironment
                {
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_3,
                    Privileged = true
                }, 
                EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                {
                    {"REPO_ECR",new BuildEnvironmentVariable{Value = containerRepository.RepositoryUri}}
                    
                }
            });
            var dockerBuildArm64 = new  Amazon.CDK.AWS.CodeBuild.PipelineProject(this, "DockerBuild_ARM64", new  Amazon.CDK.AWS.CodeBuild.PipelineProjectProps
            {
                BuildSpec = buildSpecArm64,
                Environment = new BuildEnvironment
                {
                    BuildImage = LinuxArmBuildImage.AMAZON_LINUX_2_STANDARD_1_0,
                    Privileged = true
                }, 
                EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                {
                    {"REPO_ECR",new BuildEnvironmentVariable{Value = containerRepository.RepositoryUri}}
                    
                }
            });
            
            var manifestBuild = new  Amazon.CDK.AWS.CodeBuild.PipelineProject(this, "ManifestBuild", new  Amazon.CDK.AWS.CodeBuild.PipelineProjectProps
            {
                BuildSpec = buildSpecManifest,
                Environment = new BuildEnvironment
                {
                    BuildImage = LinuxBuildImage.AMAZON_LINUX_2_3,
                    Privileged = true
                }, 
                EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                {
                    {"REPO_ECR",new BuildEnvironmentVariable{Value = containerRepository.RepositoryUri}}
                    
                }
            });

            containerRepository.GrantPullPush(dockerBuild86);
            containerRepository.GrantPullPush(dockerBuildArm64);
            containerRepository.GrantPullPush(manifestBuild);
            
         
            
            dockerBuild86.AddToRolePolicy(CreatePolicyStatementForBuild());
            dockerBuildArm64.AddToRolePolicy(CreatePolicyStatementForBuild());
            manifestBuild.AddToRolePolicy(CreatePolicyStatementForBuild());

            var sourceAction = new Amazon.CDK.AWS.CodePipeline.Actions.CodeCommitSourceAction(new CodeCommitSourceActionProps()
            {
                ActionName ="CodeCommit_Source",
                Repository = codecommitRepo,
                Output = sourceOutput,
                Branch = "master"
            });
            pipeline.AddStage(new StageOptions()
            {
                StageName = "Source",
                Actions = new[] { sourceAction }
            });

            var codeBuildAction86 =
                new Amazon.CDK.AWS.CodePipeline.Actions.CodeBuildAction(new CodeBuildActionProps()
                {
                    ActionName = "DockerBuild_x86",
                    Project = dockerBuild86,
                    Input = sourceOutput,
                    Outputs = new []{dockerOutput86}
                    
                });
            
            var codeBuildActionArm64 =
                new Amazon.CDK.AWS.CodePipeline.Actions.CodeBuildAction(new CodeBuildActionProps()
                {
                    ActionName = "DockerBuild_ARM64",
                    Project = dockerBuildArm64,
                    Input = sourceOutput,
                    Outputs = new []{dockerOutputArm64}
                    
                });
            
            pipeline.AddStage(new StageOptions()
            {
                StageName = "DockerBuild",
                Actions = new []{codeBuildAction86,codeBuildActionArm64}
            });
            
            var codeBuildActionManifest =
                new Amazon.CDK.AWS.CodePipeline.Actions.CodeBuildAction(new CodeBuildActionProps()
                {
                    ActionName = "Manifest",
                    Project = manifestBuild,
                    Input = sourceOutput,
                    Outputs = new []{manifestOutput}
                    
                });

            pipeline.AddStage(new StageOptions()
            {
                StageName = "Manifest",
                Actions = new []{codeBuildActionManifest}
            });
            
            CfnOutput = new CfnOutput(this, "application_repository", new CfnOutputProps()
            {
                Value = codecommitRepo.RepositoryCloneUrlHttp
            });

        }

        private PolicyStatement CreatePolicyStatementForBuild()
        {
            PolicyStatement policyStatement = new Amazon.CDK.AWS.IAM.PolicyStatement
            {
                Effect = Amazon.CDK.AWS.IAM.Effect.ALLOW
            };
            policyStatement.AddActions("ecr:BatchCheckLayerAvailability", "ecr:GetDownloadUrlForLayer", "ecr:BatchGetImage");
            policyStatement.AddResources($"arn:{ this.Partition}:ecr:{this.Region}:{this.Account}:repository/*");
            return policyStatement;
        }
    }
        
    
}
