using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionBuilding;

public class QueryBuilder
{
    public IQueryable<T> BuildQuery<T>(IQueryable<T> queryable, Filter filter)
    {
        return queryable.Where(BuildQueryExpression<T>(filter));
    }
    
    public Expression<Func<T, bool>> BuildQueryExpression<T>(Filter filter)
    {       
        // .Where(x => x.prop1 == value1 && x.prop2 == value2
        var arg = Expression.Parameter(typeof(T), "x");

        var body = BuildQueryExpressionInternal<T>(arg, filter);
        return Expression.Lambda<Func<T, bool>>(body, arg);
    }

    private Expression BuildQueryExpressionInternal<T>(ParameterExpression arg, Filter filter)
    {
        if (filter is SimpleFilter sf)
            return BuildQuerySimpleExpressionInternal<T>(arg, sf);
        else if (filter is ComplexFilter cf)
        {
            Expression? body = null;
            foreach (var f in cf.Filters)
            {
                var op = BuildQueryExpressionInternal<T>(arg, f);
                body = body != null ? Expression.AndAlso(body, op) : op;
            }

            return body ?? Expression.Constant(true);
        }

        throw new NotImplementedException();
    }

    private Expression BuildQuerySimpleExpressionInternal<T>(ParameterExpression arg, SimpleFilter sf)
    {
        var left = Expression.PropertyOrField(arg, sf.Property);
        return sf.Operator switch
        {
            "$gt" => Expression.GreaterThan(left, Expression.Constant(Parse(sf.Value, typeof(T).GetProperty(sf.Property)))),
            "$eq" => Expression.Equal(left, Expression.Constant(sf.Value)),
            _ => throw new NotImplementedException()
        };
    }

    private object? Parse(string sfValue, PropertyInfo propInfo)
    {
        if (propInfo.PropertyType == typeof(int))
            return int.Parse(sfValue);
        else if (propInfo.PropertyType == typeof(float))
            return float.Parse(sfValue);
        throw new NotImplementedException();
    }
}