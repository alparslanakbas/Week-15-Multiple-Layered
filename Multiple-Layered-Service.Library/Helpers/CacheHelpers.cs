namespace Multiple_Layered_Service.Library.Helpers
{
    public static class CacheHelpers
    {
        public static string GenerateCacheKey(string baseKey, Pagination pagination)
            => $"{baseKey}_page{pagination.Page}_size{pagination.Size}";
    }
}
