using HAMS.Domain;
using HAMS.Repository.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAMS.Repository.Abstract
{
    public interface IUnitOfWork
    {
        IRepository<AspNetRoles> AspNetRoleRepository { get; }
        IRepository<AspNetUsers> AspNetUserRepository { get; }
        IRepository<Comment> CommentRepository { get; }
        IRepository<Group> GroupRepository { get; }
        IRepository<StudentsGroup> StudentsGroupRepository { get; }
        IRepository<StudentsTask> StudentsTaskRepository { get; }
        IRepository<Subject> SubjectRepository { get; }
        IRepository<Domain.Task> TaskRepository { get; }
        IRepository<TaskDetail> TaskDetailRepository { get; }
        IRepository<TeachersGroup> TeachersGroupRepository { get; }

        void Save();
        void Dispose();

    }
}
