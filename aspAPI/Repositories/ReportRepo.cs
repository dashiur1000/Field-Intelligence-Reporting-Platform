using aspAPI.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Esql.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpCompress.Compressors.ZStandard.Unsafe;

namespace aspAPI.Repositories
{
    public class ReportRepo : IReportRepo
    {
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly string IndicesName = "field-reports-index";
        public ReportRepo(ElasticsearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
        }
        public async Task<List<Report>> searchAsync(
            string? text,
            string? theater,
            string? sector,
            string? location,
            List<string>? priorities,
            string? reportType,
            DateTime? from,
            DateTime? to)
        {
            var response = await _elasticsearchClient.SearchAsync<Report>(s => s
                .Indices("field-reports-index")
                .Query(q => q
                    .Bool(b =>
                    {
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            b.Must(m => m
                                .Match(ma => ma
                                    .Field("message")
                                    .Query(text)
                                )
                            );
                        }

                        if (!string.IsNullOrWhiteSpace(theater))
                        {
                            b.Filter(f => f.Term(t => t.Field("theater.keyword").Value(theater)));
                        }

                        if (!string.IsNullOrWhiteSpace(sector))
                        {
                            b.Filter(f => f.Term(t => t.Field("sector.keyword").Value(sector)));
                        }

                        if (!string.IsNullOrWhiteSpace(location))
                        {
                            b.Filter(f => f.Term(t => t.Field("location.keyword").Value(location)));
                        }

                        if (!string.IsNullOrWhiteSpace(reportType))
                        {
                            b.Filter(f => f.Term(t => t.Field("reportType.keyword").Value(reportType)));
                        }

                        if (priorities != null && priorities.Any())
                        {
                            b.Filter(f => f.Terms(ts => ts
                            .Field("priority.keyword")
                            .Terms(new TermsQueryField(priorities.Select(p => (FieldValue)p).ToList()))
                        ));
                        }

                        if (from.HasValue || to.HasValue)
                        {
                            b.Filter(f => f.Range(r => r
                                .DateRange(dr =>
                                {
                                    dr.Field("processedAt");
                                    if (from.HasValue) dr.Gte(from.Value);
                                    if (to.HasValue) dr.Lte(to.Value);
                                })
                            ));
                        }
                    })
                )
            );

            if (!response.IsValidResponse)
            {
                Console.WriteLine($"ES Error: {response.DebugInformation}");
                return new List<Report>();
            }

            return response.Documents.ToList();
        }
        public async Task<List<Report>> GetBysubjectIdAsync(string id)
        {
            var response = await _elasticsearchClient.SearchAsync<Report>(s => s
                .Indices("field-reports-index")
                .Query(q => q
                    .Term(m => m
                        .Field("subjectId.keyword")
                        .Value(id)
                        )
                    )
                .Sort(s => s.Field(f => f.processedAt))
                );
            if(!response.IsValidResponse)
            {
                return new List<Report>();
            }
            return response.Documents.ToList();
        }
        public async Task<List<Report>> GetFilteredReportsAsync(
            string? theater,
            string? sector,
            string? location,
            string? priorities,
            DateTime? from,
            DateTime? to)
        {
            var response = await _elasticsearchClient.SearchAsync<Report>(s => s
                .Indices("field-reports-index")
                .Query(q => q
                    .Bool(b =>
                    {
                        if (!string.IsNullOrWhiteSpace(theater))
                        {
                            b.Filter(f => f.Term(t => t.Field("theater.keyword").Value(theater)));
                        }

                        if (!string.IsNullOrWhiteSpace(sector))
                        {
                            b.Filter(f => f.Term(t => t.Field("sector.keyword").Value(sector)));
                        }

                        if (!string.IsNullOrWhiteSpace(location))
                        {
                            b.Filter(f => f.Term(t => t.Field("location.keyword").Value(location)));
                        }

                        if (priorities != null && priorities.Any())
                        {
                            b.Filter(f => f.Terms(ts => ts
                                .Field("priority.keyword")
                                .Terms(new TermsQueryField(priorities.Select(p => (FieldValue)p).ToArray()))
                            ));
                        }

                        if (from.HasValue || to.HasValue)
                        {
                            b.Filter(f => f.Range(r => r
                                .DateRange(dr =>
                                {
                                    dr.Field("processedAt");
                                    if (from.HasValue) dr.Gte(from.Value);
                                    if (to.HasValue) dr.Lte(to.Value);
                                })
                            ));
                        }
                    })
                )
            );

            if (!response.IsValidResponse)
            {
                Console.WriteLine($"ES Error: {response.DebugInformation}");
                return new List<Report>();
            }

            return response.Documents.ToList();
        }
        public async Task<ReportsSummaryDto> GetReportsSummaryAsync()
        {
            var response = await _elasticsearchClient.SearchAsync<Report>(s => s
                .Indices("field-reports-index")
                .Size(0)
                .Aggregations(a =>
                {
                    a.Add("priorities_agg", new TermsAggregation { Field = "priority.keyword" });
                    a.Add("theaters_agg", new TermsAggregation { Field = "theater.keyword" });
                    a.Add("types_agg", new TermsAggregation { Field = "reportType.keyword" });
                })
            );

            if (!response.IsValidResponse)
            {
                Console.WriteLine($"ES Error: {response.DebugInformation}");
                return new ReportsSummaryDto();
            }

            var summary = new ReportsSummaryDto
            {
                TotalReports = response.Total,
                ByPriority = response.Aggregations.GetStringTerms("priorities_agg")?
                    .Buckets.ToDictionary(k => k.Key.ToString()!, v => v.DocCount) ?? new(),
                ByTheater = response.Aggregations.GetStringTerms("theaters_agg")?
                    .Buckets.ToDictionary(k => k.Key.ToString()!, v => v.DocCount) ?? new(),
                ByReportType = response.Aggregations.GetStringTerms("types_agg")?
                    .Buckets.ToDictionary(k => k.Key.ToString()!, v => v.DocCount) ?? new()
            };

            return summary;
        }
        
    }
}
