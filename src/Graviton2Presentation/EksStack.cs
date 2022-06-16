using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.EKS;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace Graviton2Presentation;

public class EksStack:Stack
{
    public Cluster Cluster { get; private set; }
    public Nodegroup Nodegroup86 { get; private set; }
    public Nodegroup NodegroupArm64 { get; private set; }
    internal EksStack(Construct scope, string id, Vpc vpc , IStackProps props = null) : base(scope, id,
        props)
    {
        var eksSecurityGroup = new SecurityGroup(this, "EKSSecurityGroup", new SecurityGroupProps()
        {
            Vpc = vpc,
            AllowAllOutbound = true
        });
        eksSecurityGroup.AddIngressRule(Peer.Ipv4("10.0.0.0/16"), Port.AllTraffic());
        Cluster = new Cluster(this, "EKSGraviton2", new ClusterProps()
        {
            Version = KubernetesVersion.V1_20,
            DefaultCapacity = 0,
            EndpointAccess = EndpointAccess.PUBLIC_AND_PRIVATE,
            Vpc = vpc,
            SecurityGroup = eksSecurityGroup
        });
        Nodegroup86 = Cluster.AddNodegroupCapacity("x86-node-group", new NodegroupProps()
        {
            InstanceTypes = new []{new InstanceType("m5.large") },
            DesiredSize = 2,
            MinSize = 1,
            MaxSize = 3
        });
        
        NodegroupArm64 = Cluster.AddNodegroupCapacity("arm64-node-group", new NodegroupProps()
        {
            InstanceTypes = new []{new InstanceType("m6g.large") },
            DesiredSize = 2,
            MinSize = 1,
            MaxSize = 3
        });
        

    }
}