using System;

namespace Ois.TaskQueues
{
    [Flags]
    public enum TaskQueueServiceEvents : uint
    {
        None = 0,

        ClientConnected = 1,

        QueueCreated = 2,

        TaskCreated = 4,

        BarrierCreated = 8,

        ClientDisconnected = 16,

        WorkerConnected = 32,

        TaskAssigned = 64,

        TaskFinished = 128,

        BarrierFinished = 256,

        QueueEmptied = 512,

        QueueRemoved = 1024,

        WorkerDisconnected = 2048,

        All = 4095
    }
}