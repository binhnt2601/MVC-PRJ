using System.Net;

namespace App.ExtendedMethod
{
    public static class AppExtendedMethod
    {
        public static void AddStatusCodePage(this IApplicationBuilder app)
        {
            app.UseStatusCodePages(appError =>
            {
                appError.Run(async context =>
                {
                    var response = context.Response;
                    var code = response.StatusCode;

                    var content = @$"
                        <html>
                            <head>
                                <meta charset = 'UTF8' />
                                <title>Error {code}</title>
                            </head>
                            <body>
                                <p style= 'color: red; font-size: 35px'>
                                    Error {code}: {(HttpStatusCode)code}
                                </p>
                            </body>
                        </html>";
                    await response.WriteAsync(content);
                });
            });
        }
    }
}
