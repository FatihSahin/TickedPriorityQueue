using System;
using System.Collections.Generic;

namespace TickedPriorityQueue
{
	/// <summary>
	/// A class which manages ITicked objects, ticking them in order of priority.
	/// </summary>
	/// <remarks>
	/// Will never tick an item more than once in a frame, and sets the updated tick
	/// time to the sum of processed time and the object's Tick Length.
	/// And and Update can use a user provided DateTime for the current time, allowing for custom timing, e.g. for pausing the game.
	/// </remarks>>
	public sealed class TickedQueue
	{
		/// <summary>
		/// Default max ITicked objects to be processed per update.
		/// </summary>
		public const int DefaultMaxProcessedPerUpdate = 10;
		
		private List<TickedQueueItem> _queue;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TickedPriorityQueue.TickedQueue"/> class.
		/// </summary>
		public TickedQueue ()
		{
			_queue = new List<TickedQueueItem>();
			MaxProcessedPerUpdate = DefaultMaxProcessedPerUpdate;
		}
		
		/// <summary>
		/// Gets or sets the max ITicked objects to be processed in a single Update call.
		/// </summary>
		/// <value>
		/// The number of ITicked objects which will be processed in an Update.
		/// </value>
		public int MaxProcessedPerUpdate { get; set; }
		
		/// <summary>
		/// Gets the internal queue count.
		/// </summary>
		public int QueueCount { get { return _queue.Count; } }
		
		/// <summary>
		/// Add the specified ticked object to the queue.
		/// </summary>
		/// <param name='ticked'>
		/// The ITicked object.
		/// </param>
		public void Add(ITicked ticked)
		{
			Add(ticked, DateTime.UtcNow);
		}
		
		/// <summary>
		/// Add the specified ticked object to the queue, using currentTime as the time to use for the tick check.
		/// </summary>
		/// <param name='ticked'>
		/// The ITicked object.
		/// </param>
		/// <param name='currentTime'>
		/// Current time. Doesn't have to be the real time.
		/// </param>
		public void Add(ITicked ticked, DateTime currentTime)
		{
			TickedQueueItem item = new TickedQueueItem(ticked, currentTime);
			int index = _queue.BinarySearch(item, new TickedQueueItemComparer());
			
			//if the binary search doesn't find something identical, it'll return a
			//negative value signifying where the new item should reside, so bitflipping
			//that gives the new index
			if (index < 0) index = ~index;
			_queue.Insert(index, item);
		}
		
		/// <summary>
		/// Remove the specified ticked object from the queue.
		/// Will only remove the same object once, even if multiple instances exist.
		/// </summary>
		/// <param name='ticked'>
		/// The ITicked object to remove.
		/// </param>
		public void Remove(ITicked ticked)
		{
			foreach(var item in _queue)
			{
				if (item.ContainsTicked(ticked))
				{
					_queue.Remove(item);
					break;
				}
			}
		}
		
		/// <summary>
		/// Updates the queue, calling OnTicked for the first MaxProcessedPerUpdate items which have timed out.
		/// </summary>
		public void Update()
		{
			Update(DateTime.UtcNow);
		}
		
		/// <summary>
		/// Updates the queue, calling OnTicked for the first MaxProcessedPerUpdate items which have timed out.
		/// Uses a user provided DateTime for the current time, allowing for custom timing, e.g. for pausing the game.
		/// </summary>
		/// <param name='currentTime'>
		/// Current time to use.
		/// </param>
		public void Update(DateTime currentTime)
		{
			int found = 0;
						
			foreach(var item in _queue)
			{
				if (found > MaxProcessedPerUpdate) break;
				
				if (item.CheckTickReady(currentTime))
				{
					++found;
					item.Tick(currentTime);
				}
			}
		}
	}
}
