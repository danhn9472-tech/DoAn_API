﻿using DoAn_API.Entities;

namespace DoAn_API.Services
{
    public interface IPostDeletionService
    {
        void QueueFullPostDeletion(Post post);
        void RestorePost(Post post);
    }
}