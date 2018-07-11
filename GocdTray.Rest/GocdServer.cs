﻿using System;
using System.Collections.Generic;
using System.Linq;
using GocdTray.App.Abstractions;
using GocdTray.Rest.Dto;

namespace GocdTray.Rest
{
    public class GocdServer : IDisposable
    {
        private readonly IRestClient restClient;

        public GocdServer(IRestClient restClient)
        {
            this.restClient = restClient;
        }

        public RestResult<List<Pipeline>> GetPipelines()
        {
            var result = restClient.Get<DtoEmbedded<DtoPipelineGroupsList>>("/go/api/dashboard", "application/vnd.go.cd.v1+json");
            if (result.HasData)
            {
                var pipelines = new List<Pipeline>();
                foreach (var dtoPipelineGroup in result.Data._embedded.PipelineGroups)
                {
                    var pipelineGroupName = dtoPipelineGroup.Name;
                    foreach (var dtoPipeline in dtoPipelineGroup._embedded.pipelines)
                    {
                        pipelines.Add(new Pipeline()
                        {
                            PipelineGroupName = pipelineGroupName,
                            Name = dtoPipeline.Name
                        });
                    }
                }
                return RestResult<List<Pipeline>>.Valid(pipelines);
            }
            else
            {
                return RestResult<List<Pipeline>>.Invalid(result.ErrorMessage);
            }
        }

        public void Dispose()
        {
            restClient?.Dispose();
        }
    }
}