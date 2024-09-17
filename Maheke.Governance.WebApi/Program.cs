using Maheke.Gov.Application.Proposals;
using Maheke.Gov.Application.Providers;
using Maheke.Gov.Infrastructure.Stellar;
using Maheke.Gov.Infrastructure.Stellar.Proposals;
using StellarDotnetSdk;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped(_ => new Server(Environment.GetEnvironmentVariable("HORIZON_URL")));
builder.Services.AddScoped(_ => new SystemAccountConfiguration(
    Environment.GetEnvironmentVariable("MAHEKE_PROPOSAL_MICROPAYMENT_SENDER_ACCOUNT_PRIVATE_KEY") ??
    throw new ApplicationException("MAHEKE_PROPOSAL_MICROPAYMENT_SENDER_ACCOUNT_PRIVATE_KEY not set"),
    Environment.GetEnvironmentVariable("MAHEKE_PROPOSAL_MICROPAYMENT_RECEIVER_ACCOUNT_PRIVATE_KEY") ??
    throw new ApplicationException("MAHEKE_PROPOSAL_MICROPAYMENT_RECEIVER_ACCOUNT_PRIVATE_KEY not set"),
    Environment.GetEnvironmentVariable("MAHEKE_ESCROW_ACCOUNT_PRIVATE_KEY") ??
    throw new ApplicationException("MAHEKE_ESCROW_ACCOUNT_PRIVATE_KEY not set"),
    Environment.GetEnvironmentVariable("MAHEKE_RESULTS_ACCOUNT_PRIVATE_KEY") ??
    throw new ApplicationException("MAHEKE_RESULTS_ACCOUNT_PRIVATE_KEY not set")));

builder.Services.AddScoped(_ => new Server(Environment.GetEnvironmentVariable("HORIZON_URL")));
builder.Services.AddScoped<ProposalService>();
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped(_ => new DateTimeProvider(DateTime.UtcNow));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
}
app.UseSwaggerUI();
app.MapSwagger();
//app.UseHttpLogging();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();


public partial class Program { }
