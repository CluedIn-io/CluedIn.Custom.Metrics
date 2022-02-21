// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationDataShard.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the OrganizationDataShard type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CluedIn.Core;
using CluedIn.Core.Accounts;
using CluedIn.Core.DataStore;

namespace CluedIn.CluedIn.Custom.Metrics.Tests.Infrastructure
{
    /// <summary>
    /// The organization data shard.
    /// </summary>
    [Table("OrganizationDataShard")]
    public class OrganizationDataShard : RelationalEntity, IOrganizationDataShard, IOrganizationContextFilteredEntity
    {
        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationDataShard"/> class.
        /// </summary>
        public OrganizationDataShard()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationDataShard" /> class.
        /// </summary>
        /// <param name="shard">The shard.</param>
        public OrganizationDataShard([NotNull] IOrganizationDataShard shard)
        {
            if (shard == null)
                throw new ArgumentNullException(nameof(shard));

            OrganizationId                                 = shard.OrganizationId;
            BlobDataStoreConnectionStringName              = shard.BlobDataStoreConnectionStringName;
            ConfigurationDataStoreConnectionStringName     = shard.ConfigurationDataStoreConnectionStringName;
            DataDataStoreConnectionStringName              = shard.DataDataStoreConnectionStringName;
            ExternalSearchDataStoreConnectionStringName    = shard.ExternalSearchDataStoreConnectionStringName;
            SearchDataStoreConnectionStringName            = shard.SearchDataStoreConnectionStringName;
            GraphDataStoreReadConnectionStringName         = shard.GraphDataStoreReadConnectionStringName;
            GraphDataStoreWriteConnectionStringName        = shard.GraphDataStoreWriteConnectionStringName;
            MemoryDataStoreConnectionStringName            = shard.MemoryDataStoreConnectionStringName;
            MetricsDataStoreConnectionStringName           = shard.MetricsDataStoreConnectionStringName;
            TrainingDataStoreConnectionStringName          = shard.TrainingDataStoreConnectionStringName;
        }

        /**********************************************************************************************************
         * PROPERTIES
         **********************************************************************************************************/

        /// <summary>Gets the identifier.</summary>
        /// <value>The identifier.</value>
        [NotMapped]
        public override Guid Id
        {
            get
            {
                return OrganizationId;
            }

            set
            {
                OrganizationId = value;
            }
        }

        /// <summary>Gets or sets the organization identifier.</summary>
        /// <value>The organization identifier.</value>
        [Key]
        [Required]
        public Guid OrganizationId { get; set; }

        /// <summary>Gets or sets the BLOB data store connection string.</summary>
        /// <value>The BLOB data store connection string.</value>
        [Required]
        [Column("BlobDataStoreConnectionString")]
        public string BlobDataStoreConnectionStringName { get; set; }

        /// <summary>Gets or sets the configuration data store connection string.</summary>
        /// <value>The configuration data store connection string.</value>
        [Required]
        [Column("ConfigurationDataStoreConnectionString")]
        public string ConfigurationDataStoreConnectionStringName { get; set; }

        /// <summary>Gets or sets the data store connection string.</summary>
        /// <value>The data store connection string.</value>
        [Required]
        [Column("DataDataStoreConnectionString")]
        public string DataDataStoreConnectionStringName { get; set; }

        /// <summary>
        /// Gets or sets the name of the external search data store connection string.
        /// </summary>
        /// <value>The name of the external search data store connection string.</value>
        [Required]
        [Column("ExternalSearchDataStoreConnectionStringName")]
        public string ExternalSearchDataStoreConnectionStringName { get; set; }

        /// <summary>Gets or sets the search data store connection string.</summary>
        /// <value>The search data store connection string.</value>
        [Required]
        [Column("SearchDataStoreConnectionString")]
        public string SearchDataStoreConnectionStringName { get; set; }

        /// <summary>Gets or sets the graph data store read connection string.</summary>
        /// <value>The graph data store read connection string.</value>
        [Required]
        [Column("GraphDataStoreReadConnectionString")]
        public string GraphDataStoreReadConnectionStringName { get; set; }

        /// <summary>Gets or sets the graph data store write connection string.</summary>
        /// <value>The graph data store write connection string.</value>
        [Required]
        [Column("GraphDataStoreWriteConnectionString")]
        public string GraphDataStoreWriteConnectionStringName { get; set; }

        /// <summary>Gets or sets the memory data store connection string.</summary>
        /// <value>The memory data store connection string.</value>
        [Required]
        [Column("MemoryDataStoreConnectionString")]
        public string MemoryDataStoreConnectionStringName { get; set; }

        [Required]
        [Column("MetricsDataStoreConnectionStringName")]
        public string MetricsDataStoreConnectionStringName { get; set; }

        [Required]
        [Column("TrainingDataStoreConnectionString")]
        public string TrainingDataStoreConnectionStringName { get; set; }

        /// <inheritdoc/>
        public string GetDataShardConnectionStringName(DataShardType dataShardType)
        {
            switch (dataShardType)
            {
                case DataShardType.Metrics:
                    return MetricsDataStoreConnectionStringName;

                case DataShardType.Blob:
                    return BlobDataStoreConnectionStringName;

                case DataShardType.Configuration:
                    return ConfigurationDataStoreConnectionStringName;

                case DataShardType.Data:
                    return DataDataStoreConnectionStringName;

                case DataShardType.External:
                    return ExternalSearchDataStoreConnectionStringName;

                case DataShardType.Graph:
                    return GraphDataStoreReadConnectionStringName;

                case DataShardType.Memory:
                    return MemoryDataStoreConnectionStringName;

                case DataShardType.Search:
                    return SearchDataStoreConnectionStringName;

                case DataShardType.Training:
                    return TrainingDataStoreConnectionStringName;

                case DataShardType.All:
                    throw new InvalidOperationException("Cannot return a connection string for all data shard types");

                default:
                    throw new InvalidEnumArgumentException(nameof(dataShardType), (int)dataShardType, typeof(DataShardType));
            }
        }

        /// <summary>Gets or sets the organization account.</summary>
        /// <value>The organization account.</value>
        [ForeignKey("OrganizationId")]
        public OrganizationAccount OrganizationAccount { get; set; }
    }
}
