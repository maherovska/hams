using HAMS.Domain;
using HAMS.Repository.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAMS.Repository.Concrete
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private HAMS_DB_AspNetEntities _context;

        private IRepository<AspNetRoles> _rolesRepository;
        private IRepository<AspNetUsers> _usersRepository;
        private IRepository<Comment> _commentRepository;
        private IRepository<Group> _groupRepository;
        private IRepository<StudentsGroup> _studentsGroupRepository;
        private IRepository<StudentsTask> _studentsTaskRepository;
        private IRepository<Subject> _subjectRepository;
        private IRepository<Domain.Task> _taskRepository;
        private IRepository<TaskDetail> _taskDetailRepository;
        private IRepository<TeachersGroup> _teachersGroupRepository;

        public UnitOfWork()
        {
            _context = new HAMS_DB_AspNetEntities();
        }

        public IRepository<AspNetRoles> AspNetRoleRepository
        {
            get
            {
                if (this._rolesRepository == null)
                {
                    this._rolesRepository = new GenericRepository<AspNetRoles>(_context);
                }
                return _rolesRepository;
            }
        }

        public IRepository<AspNetUsers> AspNetUserRepository
        {
            get
            {

                if (this._usersRepository == null)
                {
                    this._usersRepository = new GenericRepository<AspNetUsers>(_context);
                }
                return _usersRepository;
            }
        }

        public IRepository<Comment> CommentRepository
        {
            get
            {

                if (this._commentRepository == null)
                {
                    this._commentRepository = new GenericRepository<Comment>(_context);
                }
                return _commentRepository;
            }
        }

        public IRepository<Group> GroupRepository
        {
            get
            {

                if (this._groupRepository == null)
                {
                    this._groupRepository = new GenericRepository<Group>(_context);
                }
                return _groupRepository;
            }
        }

        public IRepository<StudentsGroup> StudentsGroupRepository
        {
            get
            {

                if (this._studentsGroupRepository == null)
                {
                    this._studentsGroupRepository = new GenericRepository<StudentsGroup>(_context);
                }
                return _studentsGroupRepository;
            }
        }

        public IRepository<StudentsTask> StudentsTaskRepository
        {
            get
            {

                if (this._studentsTaskRepository == null)
                {
                    this._studentsTaskRepository = new GenericRepository<StudentsTask>(_context);
                }
                return _studentsTaskRepository;
            }
        }

        public IRepository<Subject> SubjectRepository
        {
            get
            {

                if (this._subjectRepository == null)
                {
                    this._subjectRepository = new GenericRepository<Subject>(_context);
                }
                return _subjectRepository;
            }
        }

        public IRepository<Domain.Task> TaskRepository
        {
            get
            {

                if (this._taskRepository == null)
                {
                    this._taskRepository = new GenericRepository<Domain.Task>(_context);
                }
                return _taskRepository;
            }
        }

        public IRepository<TaskDetail> TaskDetailRepository
        {
            get
            {

                if (this._taskDetailRepository == null)
                {
                    this._taskDetailRepository = new GenericRepository<TaskDetail>(_context);
                }
                return _taskDetailRepository;
            }
        }

        public IRepository<TeachersGroup> TeachersGroupRepository
        {
            get
            {

                if (this._teachersGroupRepository == null)
                {
                    this._teachersGroupRepository = new GenericRepository<TeachersGroup>(_context);
                }
                return _teachersGroupRepository;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
