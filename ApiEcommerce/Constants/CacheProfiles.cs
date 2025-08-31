using System;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Constants;

public class CacheProfiles
{
    public const string Default10 = "Default10";
    public const string Default30 = "Default30";

    public static readonly CacheProfile Profile10 = new CacheProfile { Duration = 10 };
    public static readonly CacheProfile Profile30 = new CacheProfile { Duration = 30 };
}
