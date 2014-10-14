﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.ImportExport.Models;
using Orchard.ImportExport.Services;
using Orchard.Services;

namespace Orchard.ImportExport.DeploymentTargets {
    public class RemoteOrchardDeploymentSource : IDeploymentSource, IDeploymentSourceProvider {
        private RemoteOrchardDeploymentPart DeploymentPart { get; set; }
        private Lazy<RemoteOrchardApiClient> Client { get; set; }
        private readonly ISigningService _signingService;
        private readonly IClock _clock;
        private readonly UrlHelper _url;

        public RemoteOrchardDeploymentSource(ISigningService signingService, IClock clock, UrlHelper url) {
            _signingService = signingService;
            _clock = clock;
            _url = url;
        }

        public DeploymentSourceMatch Match(IContent sourceConfiguration) {
            if (!sourceConfiguration.Is<RemoteOrchardDeploymentPart>()) return null;
            
            DeploymentPart = sourceConfiguration.As<RemoteOrchardDeploymentPart>();
            Client = new Lazy<RemoteOrchardApiClient>(() => new RemoteOrchardApiClient(DeploymentPart, _signingService, _clock));
            return new DeploymentSourceMatch {DeploymentSource = this, Priority = 0};
        }

        public string GetRecipe(RecipeRequest request) {
            var actionUrl = _url.Action("Recipe", "Export", new {
                area = "Orchard.ImportExport"
            });
            var data = JsonConvert.SerializeObject(request);
            return Client.Value.Post(actionUrl, data);
        }

        public IList<DeploymentContentType> GetContentTypes() {
            var actionUrl = _url.Action("ContentTypes", "Export", new {
                area = "Orchard.ImportExport"
            });
            var result = Client.Value.Get(actionUrl);
            return JsonConvert.DeserializeObject<List<DeploymentContentType>>(result);
        }

        public IList<DeploymentQuery> GetQueries() {
            var actionUrl = _url.Action("Queries", "Export", new {
                area = "Orchard.ImportExport"
            });
            var result = Client.Value.Get(actionUrl);
            return JsonConvert.DeserializeObject<List<DeploymentQuery>>(result);
        }
    }
}
