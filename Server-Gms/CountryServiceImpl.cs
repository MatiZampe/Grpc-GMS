using Country;
using Grpc.Core;
using Npgsql;
using System;
using System.Threading.Tasks;
using static Country.CountryService;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;

namespace Server_Gms
{
    internal class CountryServiceImpl : CountryServiceBase
    {
        private readonly string _connectionString;

        public CountryServiceImpl(string connectionString)
        {
            _connectionString = connectionString;
        }


        public override async Task<ReadCountryResponse> ReadCountry(ReadCountryRequest request, ServerCallContext context)
        {
            var countryId = request.CountryId;
            var country = await GetCountryFromDatabaseAsync(countryId);

            if (country == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"The country id {countryId} wasn't found"));

            return new ReadCountryResponse
            {
                Country = new Country.Country()
                {
                    Id = country.Id,
                    Name = country.Name,
                    Code = country.Code
                }
            };
        }


        public override async Task ListCountry(ListCountryRequest request, IServerStreamWriter<ListCountryResponse> responseStream, ServerCallContext context)
        {
            var countries = await GetAllCountriesFromDatabaseAsync();

            foreach (var country in countries)
            {
                await responseStream.WriteAsync(new ListCountryResponse()
                {
                    Country = new Country.Country
                    {
                        Id = country.Id,
                        Name = country.Name,
                        Code = country.Code
                    }
                });
            }
        }


        private async Task<CountryRecord> GetCountryFromDatabaseAsync(int countryId)
        {
            CountryRecord country = null;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT country_id, name, code FROM countries WHERE country_id = @countryId", connection))
                {
                    command.Parameters.AddWithValue("countryId", countryId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            country = new CountryRecord
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Code = reader.GetString(2)
                            };
                        }
                    }
                }
            }

            return country;
        }


        private async Task<List<CountryRecord>> GetAllCountriesFromDatabaseAsync()
        {
            var countries = new List<CountryRecord>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand("SELECT country_id, name, code FROM countries", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            countries.Add(new CountryRecord
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Code = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return countries;
        }

        // Define a class to map the database record
        private class CountryRecord
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Code { get; set; }
        }
    }
}