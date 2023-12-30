// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Meshy
{
    public readonly struct TaskProgress
    {
        public static implicit operator TaskProgress(MeshyTaskResult result)
            => result != null ? new TaskProgress(Guid.Parse(result.Id), result.Status, result.Progress, result.PrecedingTasks) : default;

        public TaskProgress(Guid id, Status status, int progress, int? precedingTasks)
        {
            Id = id;
            Status = status;
            Progress = progress;
            PrecedingTasks = precedingTasks;
        }

        public Guid Id { get; }

        public Status Status { get; }

        public int Progress { get; }

        public int? PrecedingTasks { get; }
    }
}
