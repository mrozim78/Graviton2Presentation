using Amazon.CDK;
using System.Collections.Generic;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace Graviton2Presentation;

public class VpcStack: Stack
{
    public Vpc Vpc { get; private set; }
    internal VpcStack(Construct scope, string id, IStackProps props = null) : base(scope, id,
        props)
    {
        var subnetConfigurationPublic = new SubnetConfiguration
        {
            Name = "Public",
            SubnetType = SubnetType.PUBLIC,
            CidrMask = 24
        };

        var subnetConfigurationPrivate = new SubnetConfiguration()
        {
            Name = "Private",
            SubnetType = SubnetType.PRIVATE_WITH_NAT,
            CidrMask = 24
        };
        
        Vpc = new Vpc(this, "BaseVPC", new VpcProps()
        {
            MaxAzs = 2,
            Cidr = "10.0.0.0/16",
            EnableDnsSupport = true,
            EnableDnsHostnames = true,
            SubnetConfiguration = new []{subnetConfigurationPublic,subnetConfigurationPrivate},
            NatGateways = 2
        });
    }
}