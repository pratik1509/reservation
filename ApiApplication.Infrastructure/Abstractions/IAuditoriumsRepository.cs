﻿using ApiApplication.Domain.Entities;

namespace ApiApplication.Infrastructure.Abstractions
{
    public interface IAuditoriumsRepository
    {
        Task<AuditoriumEntity?> GetAsync(int auditoriumId, CancellationToken cancel);
    }
}