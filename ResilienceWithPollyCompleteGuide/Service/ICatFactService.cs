namespace ResilienceWithPollyCompleteGuide.Service
{
    public interface ICatFactService 
    {
        Task<string> GetDailyFact();
    }
}
