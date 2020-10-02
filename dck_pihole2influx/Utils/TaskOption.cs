using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Optional;

namespace dck_pihole2influx.Utils
{
public class TaskOption<T>
{
	private readonly Func<Task<T>> _task;

	public TaskOption(Task<T> task)
		: this(() => task)
	{ }

	public TaskOption(Func<Task<T>> task)
	{
		_task = task ?? throw new ArgumentNullException(nameof(task));
	}

	public TaskOption<T> Filter(Predicate<T> filterPredicate, Func<T, Exception> exceptionalFunc)
	{
		return Match(
			some: s => filterPredicate(s) ? s : throw exceptionalFunc(s),
			none: n => throw n);
	}

	public TaskOption<TResult> Map<TResult>(Func<T, TResult> mapping) =>
		_task().ContinueWith(t => mapping(t.Result));

	public TaskOption<TResult> Map<TResult>(Func<T, Task<TResult>> mapping) =>
		_task().ContinueWith(t => mapping(t.Result)).Unwrap();

	public TaskOption<TResult> Match<TResult>(Func<T, TResult> some, Func<Exception, TResult> none) => _task()
		.ContinueWith(t =>
		{
			if (t.IsCanceled)
			{
				return none(new TaskCanceledException(t));
			}

			if (t.IsFaulted)
			{
				return none(t.Exception);
			}

			return some(t.Result);
		});

	#region Await

	public TaskAwaiter<Option<T, Exception>> GetAwaiter()
	{
		var continued = _task().ContinueWith(t =>
		{
			if (t.IsCanceled)
			{
				return Option.None<T, Exception>(new TaskCanceledException(t));
			}

			if (t.IsFaulted)
			{
				return Option.None<T, Exception>(t.Exception);
			}

			return Option.Some<T, Exception>(t.Result);
		});

		return continued.GetAwaiter();
	}

	public ConfiguredTaskAwaitable<Option<T, Exception>> ConfigureAwait(bool continueOnCapturedContext)
	{
		var continued = _task().ContinueWith(t => {
			if (t.IsCanceled)
			{
				return Option.None<T, Exception>(new TaskCanceledException(t));
			}

			if (t.IsFaulted)
			{
				return Option.None<T, Exception>(t.Exception);
			}

			return Option.Some<T, Exception>(t.Result);
		});

		return continued.ConfigureAwait(continueOnCapturedContext);
	}

	#endregion

	#region Operators

	public static implicit operator Task<T>(TaskOption<T> option) => option._task();

	public static implicit operator TaskOption<T>(Task<T> task) => new TaskOption<T>(task);

	#endregion
}
}