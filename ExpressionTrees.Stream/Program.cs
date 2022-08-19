// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ExpressionTrees.Stream.DB;
using Microsoft.EntityFrameworkCore;



// Leak Sample
for (int i = 0; i < 500; i++)
{
    using var ctx1 = new FullTextGamesContext();
    var arg = Expression.Parameter(typeof(Word), "x");
    Expression prop = Expression.PropertyOrField(arg, "Id");
    var value = new ValueHolder<int>() { Value = i };
    Expression constant = Expression.PropertyOrField(Expression.Constant(value), "Value");
    Expression body = Expression.Equal(prop, constant);

    var generated = Expression.Lambda<Func<Word, bool>>(body, arg);
    Expression<Func<Word, bool>> compiled = x => x.Id == i;
    var sql1 =  ctx1.Words.Where(generated).ToQueryString();
    var sql2 =  ctx1.Words.Where(compiled).ToQueryString();
}

Console.ReadLine();

public class ValueHolder<T>
{
    public T Value { get; set; }
}

public class Spec<T> where T : class
{
    private readonly Expression<Func<T, bool>> _expression;

    private Spec(Expression<Func<T, bool>> expression)
    {
        _expression = expression;
    }

    public bool IsSatisfiedBy(T obj) => _expression.ToLambda()(obj);

    public static Spec<T> operator |(Spec<T> left, Spec<T> right) => new (left._expression.Or(right));
    public static Spec<T> operator &(Spec<T> left, Spec<T> right) => new (left._expression.And(right));
    
    public static bool operator false(Spec<T> left) => false;

    public static bool operator true(Spec<T> left) => false;

    public static implicit operator Expression<Func<T, bool>>(Spec<T> spec)
    {
        return spec._expression;
    }

    public static implicit operator Spec<T>(Expression<Func<T, bool>> expression)
    {
        return new Spec<T>(expression);
    }
}

public class Specs
{
    public static Spec<Word> IsGoodWord => IsGoodWordExpression;
    public static Spec<Word> IsGoodId => IsGoodIdExpression;

    private static Expression<Func<Word, bool>> IsGoodWordExpression => w => w.Word1.Length > 6 && w.Word1.Length < 20;

    private static Expression<Func<Word, bool>> IsGoodIdExpression => w => w.Id < 50;
}

public static class ExpressionExtensions
{
    public static Func<TIn, TOut> ToLambda<TIn, TOut>(this Expression<Func<TIn, TOut>> exp) =>
        LambdaCache<TIn, TOut>.ToLambda(exp);
    
    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        => left.Compose(right, Expression.OrElse);
    
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        => left.Compose(right, Expression.AndAlso);
    
    private static Expression<Func<T, bool>> Compose<T>(this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right,
        Func<Expression, Expression, Expression> op)
    {
        var param = Expression.Parameter(typeof(T));
        var leftR = ParameterReplacer.Replace<Func<T, bool>, Func<T, bool>>(left, left.Parameters[0], param);
        var rightR = ParameterReplacer.Replace<Func<T, bool>, Func<T, bool>>(right, right.Parameters[0], param);
        var usingReplace = op(leftR.Body, rightR.Body);
        //var usingInvoke = op(Expression.Invoke(left, param), Expression.Invoke(right, param));
        return Expression.Lambda<Func<T, bool>>(usingReplace, param);
    } 
}


public static class LambdaCache<TIn, TOut>
{
    private static readonly ConcurrentDictionary<Expression<Func<TIn, TOut>>, Func<TIn, TOut>> _cache = new();

    public static Func<TIn, TOut> ToLambda(Expression<Func<TIn, TOut>> exp)
    {
        return _cache.GetOrAdd(exp, key =>key.Compile());
    } 
}


public static class ParameterReplacer
{
    // Produces an expression identical to 'expression'
    // except with 'source' parameter replaced with 'target' expression.     
    public static Expression<TOutput> Replace<TInput, TOutput>
    (Expression<TInput> expression,
        ParameterExpression source,
        Expression target)
    {
        return new ParameterReplacerVisitor<TOutput>(source, target)
            .VisitAndConvert(expression);
    }

    private class ParameterReplacerVisitor<TOutput> : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly Expression _target;

        public ParameterReplacerVisitor
            (ParameterExpression source, Expression target)
        {
            _source = source;
            _target = target;
        }

        internal Expression<TOutput> VisitAndConvert<T>(Expression<T> root)
        {
            return (Expression<TOutput>)VisitLambda(root);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            // Leave all parameters alone except the one we want to replace.
            var parameters = node.Parameters
                .Select(p => p == _source ? _target : p)
                .Cast<ParameterExpression>();

            return Expression.Lambda<TOutput>(Visit(node.Body), parameters);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // Replace the source with the target, visit other params as usual.
            return node == _source ? _target : base.VisitParameter(node);
        }
    }
}

//
// BenchmarkRunner.Run<CreateInstanceBenchmark>();
//
// public class CreateInstanceBenchmark
// {
//     private Lazy<Func<Person>> _lambdaLazy = new (BuildLambda);
//
//     private static Type _type = typeof(Person);
//     private static PropertyInfo _nameProp = _type.GetProperty("Name");
//     private static PropertyInfo _surnameProp = _type.GetProperty("Surname");
//
//     [Benchmark(Baseline = true)]
//     public Person CompileTime()
//     {
//         return new Person()
//         {
//             Name = "Andrii",
//             Surname = "Podkolzin"
//         };
//     }
//     
//     [Benchmark]
//     public object WithReflection2()
//     {
//         var obj = Activator.CreateInstance(_type);
//         
//         _nameProp.SetValue(obj, "Andrii");
//         _surnameProp.SetValue(obj, "Podkolzin");
//
//         return obj;
//     }
//
//     [Benchmark]
//     public object WithReflection()
//     {
//         var type = typeof(Person);
//         var obj = Activator.CreateInstance(type);
//         
//         type.GetProperty("Name").SetValue(obj, "Andrii");
//         type.GetProperty("Surname").SetValue(obj, "Podkolzin");
//
//         return obj;
//     }
//
//     [Benchmark]
//     public object WithExpressions()
//     {
//         return _lambdaLazy.Value();
//     }
//
//     private static Func<Person> BuildLambda()
//     {
//         var type = typeof(Person);
//         var creation = Expression.New(type);
//         var variable = Expression.Variable(type, "x");
//
//         var block = Expression.Block(variables: new []{ variable },
//             Expression.Assign(variable, creation),
//             Expression.Assign(Expression.PropertyOrField(variable, "Name"), Expression.Constant("Andrii")),
//             Expression.Assign(Expression.PropertyOrField(variable, "Surname"), Expression.Constant("Podkolzin")),
//             variable
//         );
//         /*
//          * var obj = new Person();
//          * obj.Name = "Andrii";
//          * obj.Surname = "Podkolzin;
//          */
//
//         var lambda = Expression.Lambda<Func<Person>>(block);
//         return lambda.Compile();
//     }
// }
//
// public class Person
// {
//     public string Name { get; set; }
//     public string Surname { get; set; }
// }