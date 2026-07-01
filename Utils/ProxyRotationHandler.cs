
using System.Net;
using UltimateParser.Utils;

public class ProxyRotationHandler : DelegatingHandler 
    {
        private readonly ProxyManager _proxyManager;

        public ProxyRotationHandler(ProxyManager proxyManager) : base(new SocketsHttpHandler { Proxy = null, UseProxy = false })
        {
            _proxyManager = proxyManager;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var proxyUrl = _proxyManager.GetNextProxy();
            
            if (!string.IsNullOrEmpty(proxyUrl) && Uri.TryCreate(proxyUrl, UriKind.Absolute, out var uri))
            {
                var webProxy = new WebProxy($"{uri.Scheme}://{uri.Host}:{uri.Port}");
                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    var userInfo = uri.UserInfo.Split(':');
                    webProxy.Credentials = new NetworkCredential(userInfo[0], userInfo.Length > 1 ? userInfo[1] : "");
                }

                if (InnerHandler is SocketsHttpHandler socketsHandler)
                {
                    socketsHandler.Proxy = webProxy;
                    socketsHandler.UseProxy = true;
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }