﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeCA
{
    public class Temp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public bool Status { get; set; }
    }
    class Program
    {   
        static HttpClient client = new HttpClient();
        

        static async Task<Temp> UpdateTempAsync(Temp temp)
        {
            HttpResponseMessage response = await client.PutAsJsonAsync($"api/TempsAPI/{temp.Id}", temp);
            response.EnsureSuccessStatusCode();

            // Deserialize the updated product from the response body.
            temp = await response.Content.ReadAsAsync<Temp>();

            return temp;
        }

        static async Task<List<Temp>> GetAllTempsAsync()
        {
            List<Temp> temps = new List<Temp>();
            
            HttpResponseMessage response = client.GetAsync(client.BaseAddress + "api/TempsAPI").Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<Temp>>().Result;
                foreach (var d in dataObjects)
                {
                    temps.Add(d);
                }
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return temps;
        }

        static Temp UpdateTempValue(Temp temp)
        {
            Random rnd = new Random();
            int dice = rnd.Next(1, 4);
            switch (dice)
            {
                case 1:
                    {
                        if (temp.Value < 35)
                        {
                            temp.Value += 1;
                        }
                        break;
                    }
                case 2:
                    {
                        if (temp.Value > -35)
                        {
                            temp.Value -= 1;
                        }
                        break;
                    }
                case 3:
                    {
                        break;
                    }
                default:
                    break;
            }
            return temp;
        }

        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            client.BaseAddress = new Uri("http://localhost:58335/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            for (;;)
            {
                try
                {
                    // TODO: fix await on GetAllTempsAsync();
                    // GET List data response.
                    var temps = await GetAllTempsAsync();
                    
                    if (temps.Count != 0)
                    {
                        for (int i = 0; i < temps.Count; i++)
                        {
                            var newTemp = UpdateTempValue(temps[i]);
                            await UpdateTempAsync(temps[i]);
                            Console.WriteLine(temps[i].Value);
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Temp List equal empty");
                        break;
                    }


                    Console.WriteLine("***************************");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Console.Read();
        }
    }
}