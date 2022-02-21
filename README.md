# CluedIn.Custom.Metrics

## Accountability
You have a record where it is composed from different places. HOW many of those places have a Procut Owner in CluedIN. To increase this, make someone the product owner of each data source in your system. We ONLY treat connectors currently as allowing this. Ingestion, Files and Databases do not support this. 

## Accuracy
You have a record that has more than one part, and properties are not aligning. You have to load up a cleaning project and do it on a record level instead of an entity level. YOu need to make them the same value BUT they can be different casing.

## Completeness
You have a record where it has a number of expected properties on it and not all values are filled in with a value. To fix, go back to your operational system and fill in those properties. Wait until the next time the data is ingested.

## Connectivity
How many records have at least one edge. If you have a low score, it meands that you have records that don't connect to any other record. Fix this my updating your mappings to have edges. 

## Dark Data
This measures how man Shadow Entities you have. Shadow Entities are pointers to records that don't exist yet e.g. You are reffering to a Movie and you have the ID, but you don't have the actual movie in your system. I high Dark Data indicates that you have to find more datasets to ingest because you are missing obvious data that exists in your business. 

## Flexiblity
Given a record, how versatile is it in being used many times for many different reasons i.e. how many streams involve this record where the TYPE of stream Export Target is different e.g. Power BI is BI, Azure ML is ML. Raise flexiblity by having streams go to different Export Targets. 

## Interpretability
How perfect is your data at aligning to a core or non provider specific Vocab. As soon as you see a record in CluedIn that has provider speciifc Vocab, this gives the impression that records are harder to understand and indicates that you are bring in data that you probably won't use becuase it is not part of a common semantic. Raise your interpretability by adding more CORE Vocablaries to map your input data. 

## Noise
This is calculated by External Data (from Enrichment Services) that are also ShadowEntities. This means that you are fdoing a lot of fuzzy lookups on records that have no Entity Codes to map back to your records or your fuzzy merging is not doing a good enough job or the data merging is not obvious. To fix this, open the Shadow Entity Studio and start to MANUALLY map records together. 

## Orderliness
This is about how pedantic you are with every data part having a matching CluedIn CLean part. This would indiciate that your team essentially does a manual check and clean on every record that comes through the platform, not just the Golden Record. To fix this, you need to basically clean every data part that has ever come through. 

## Relevance
How many of your Vocabs map to a CORE CLuedIN Vocab EVENTUALLY. To increase this score you should either not ingest data that is not mapped to core vocabulaies or you should map existing keys to Core Vocabs. 

## Sparsity
Do your records come together from a lot of places. Don't get us wrong, high spartsisty is good! It means that ClueDIn is doing its job and bring records together. A Low sparcity means that your records might not be joining well. 

## Staleness
How large are the gaps between modified dates. Stale records happen due to low rates in Modified Dates. Just because a record has not been modified, doesn't mean that it is stale. However, in our experience it is usually a sign of stale data. To lower your staleness, update your records in source systems OR in CluedIn. It could be as simple as changing a value. 

## Stewardship
This is measured by the amount of manual attention that is given to data in respect to the amount of records on an Entity. If your Clean to Input ratio is high, you have a high Stewardship. To increase this score, clean more data and clean it more often. 

## Timeliness
Find out the different between discoverability date and the use within streams. Timeliness is really about how fast it is from data entry into CluedIn until the time it is used on a stream. A high timeliness indicates that you are delivering data to your business in a quick turn around. A low Timeliness would indicate that your data sits in your MDM platform and it is a long time before it is actually activated within the business. To increase your timeliness, setup live steams of data. You may also fix this by buying more CluedIn licenses to speed up processing.

## Uniformity
Are values absolutely identicial between data parts. Will not accept any changes even casing. To increae this, create cleaning projects at the data part level and make sure that values have the exact same value across systems. 

## Usability
How many streams use this data. This simply measures the ratio of data that is in your MDM and is used by downstream consumers versus not. To make this score higher, setup more stteams of data for ALL your data in CluedIn i.e. come up with more use cases to use your data. 
