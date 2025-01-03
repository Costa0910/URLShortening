namespace URLShortening.Data.Repository;

public class
    AccessLogRepository(DataContext context) : Repository<AccessLog>(context)
    , IAccessLogRepository;
