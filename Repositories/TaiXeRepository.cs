﻿using ERP.Data;
using ERP.Infrastructure;
using ERP.Models;

namespace ERP.Repositories
{
    public interface ITaiXeRepository : IRepository<TaiXe>
    {

    }
    public class TaiXeRepository : Repository<TaiXe>, ITaiXeRepository
    {
        public TaiXeRepository(MyDbContext _db) : base(_db)
        {
        }
        public MyDbContext MyDbContext
        {
            get
            {
                return _db as MyDbContext;
            }
        }


    }
}