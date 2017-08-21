// <copyright file="Program.cs" company="DECTech.Tokyo">
// Copyright (c) DECTech.Tokyo. All rights reserved.
// </copyright>

namespace CrawlerApi
{
    using System.IO;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// App Main.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// App Main.
        /// </summary>
        /// <param name="args">Commnad line args.</param>
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseApplicationInsights()
                .UseKestrel(options =>
                {
                    // options.ThreadCount = 4;
                    // options.UseHttps("cert.pfx", "certpassword");
                })
                .UseUrls("http://+:5000" /*, "https://+:5001" */)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}