
using Microsoft.EntityFrameworkCore;

using AvaBPMS.Domain;

namespace AvaBPMS.Infrastructure.Data.Configurations
{
    public class PoolDbConfiguration : IEntityTypeConfiguration<Pool>
    {

        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Pool> builder)
        {
           
            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.HasKey(t => t.Id);            
            builder.Property(t => t.Created).IsRequired();
            builder.Property(t => t.LastModified).HasMaxLength(50).IsRequired();           
            builder.Property(t => t.CreatedBy).HasMaxLength(50).IsRequired(false);
            builder.Property(t => t.LastModifiedBy).HasMaxLength(50).IsRequired(false);
            builder.Property(t=>t.Title).HasMaxLength(256).IsRequired(true);
            
        }
    }
    public class LaneDbConfiguration : IEntityTypeConfiguration<Lane>
    {

        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Lane> builder)
        {

            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Created).IsRequired();
            builder.Property(t => t.LastModified).HasMaxLength(50).IsRequired();
            builder.Property(t => t.CreatedBy).HasMaxLength(50).IsRequired(false);
            builder.Property(t => t.LastModifiedBy).HasMaxLength(50).IsRequired(false);
            builder.Property(t => t.Title).HasMaxLength(256).IsRequired(true);
            builder.Property(t => t.RelatedRoleId).HasMaxLength(256).IsRequired(true);
            builder.Property(t => t.RelatedUserId).HasMaxLength(256).IsRequired(true);
            builder.Property(t => t.PoolId).IsRequired(true);
        }
    }
    public class WorkFlowNodeDbConfiguration : IEntityTypeConfiguration<WorkFlowNode>
    {

        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<WorkFlowNode> builder)
        {

            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Created).IsRequired();
            builder.Property(t => t.LastModified).HasMaxLength(50).IsRequired();
            builder.Property(t => t.CreatedBy).HasMaxLength(50).IsRequired(false);
            builder.Property(t => t.LastModifiedBy).HasMaxLength(50).IsRequired(false);
            builder.Property(t => t.Title).HasMaxLength(256).IsRequired(true);
            
            builder.Property(t => t.WorkFlowNodeType).IsRequired(true);
        }
    }
    public class TransitionDbConfiguration : IEntityTypeConfiguration<Transition>
    {

        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Transition> builder)
        {

            builder.Property(t => t.Id).ValueGeneratedOnAdd();
            builder.HasKey(t => t.Id);
            builder.Property(t => t.SourceWorkFlowNodeId).IsRequired();
            builder.Property(t => t.NextWorkFlowNodeId).IsRequired();
            builder.Property(t => t.Command).IsRequired(true);
            builder.Property(t => t.Title).HasMaxLength(256).IsRequired(true);
            builder.Property(t => t.TransitionCondition).IsRequired(true);

        }
    }
}
