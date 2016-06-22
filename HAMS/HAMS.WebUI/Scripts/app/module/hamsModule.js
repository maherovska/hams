var app = angular.module("hamsModule", ['ngAnimate', 'ui.bootstrap'])

app.directive('modal', function () {
    return {
        template: '<div class="modal fade">' +
            '<div class="modal-dialog">' +
              '<div class="modal-content">' +
                '<div class="modal-header">' +
                  '<button type="button" ng-click="close()" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>' +
                  '<h4 class="modal-title">{{ title }}</h4>' +
                '</div>' +
                '<div class="modal-body" ng-transclude></div>' +
              '</div>' +
            '</div>' +
          '</div>',
        restrict: 'E',
        transclude: true,
        replace: true,
        scope: true,
        link: function postLink(scope, element, attrs) {
            scope.title = attrs.title;
            scope.$watch(attrs.visible, function (value) {
                if (value == true)
                    $(element).modal('show');
                else
                    $(element).modal('hide');
            });
            $(element).on('shown.bs.modal', function () {
                scope.$apply(function () {
                    scope.$parent[attrs.visible] = true;
                });
            });
            $(element).on('hidden.bs.modal', function () {
                scope.$apply(function () {
                    scope.$parent[attrs.visible] = false;
                });
            });
        }
    };
});

app.filter('unique', function () {

    return function (items, filterOn) {

        if (filterOn === false) {
            return items;
        }

        if ((filterOn || angular.isUndefined(filterOn)) && angular.isArray(items)) {
            var hashCheck = {}, newItems = [];

            var extractValueToCompare = function (item) {
                if (angular.isObject(item) && angular.isString(filterOn)) {
                    return item[filterOn];
                } else {
                    return item;
                }
            };

            angular.forEach(items, function (item) {
                var valueToCheck, isDuplicate = false;

                for (var i = 0; i < newItems.length; i++) {
                    if (angular.equals(extractValueToCompare(newItems[i]), extractValueToCompare(item))) {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate) {
                    newItems.push(item);
                }

            });
            items = newItems;
        }
        return items;
    };
});


app.filter('dateFilter', function () {
    return function (items, order) {
        return items.filter(function (item) {
            if (order == '') {
                return true;
            } else if (order == 'active') {
                return parseInt(item['DeadLine']) >= parseInt(new Date().getTime());
            } else {
                return parseInt(item['DeadLine']) < parseInt(new Date().getTime())
            }
            })

    }
})



app.controller("datailedTaskCtrl", ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {
    $scope.taskid = window.location.href.split('=')[1]
    $scope.taskDetails = {};
    $scope.comments = [];
    $scope.showComments = false;
    $http({
        method: 'GET',
        url: '/Task/GetTaskDetail?id=' + $scope.taskid
    }).then(function successCallback(response) {
        // debugger;
        $scope.taskDetails = response.data;
        $scope.comments = $scope.taskDetails['Comments'];
        $scope.taskDetails.DeadLine = $scope.taskDetails.DeadLine;
        $scope.commentsCount = $scope.comments.length;
        $scope.minutesLeft = Math.round((parseInt($scope.taskDetails.DeadLine) - new Date().getTime()) / (1000 * 60));
        var timerID = setInterval(function () {
            $scope.minutesLeft = Math.round((parseInt($scope.taskDetails.DeadLine) - new Date().getTime()) / (1000 * 60))
        }, 1000 * 60);
        if ($scope.minutesLeft == 0) {
            clearInterval(timerID)
        }
    });
    $scope.getComments = function () {
        $scope.showComments = $scope.showComments ? false : true;
        $scope.commentsCount = $scope.comments.length;
    }

    $scope.postComment = function (comment) {
        if (!comment.Content)
            return;
           $http({
            'method': 'POST',
            'url': '/Task/Comment',
            data: {
                Content: comment.Content,
                TaskId: $scope.taskid           
            }
        }).then(function successCallback(response) {
            $http({
                method: 'GET',
                url: '/Task/GetComments?id=' + $scope.taskid
            }).then(function successCallback(response) {
                // debugger;
                $scope.comments = response.data;
                $scope.commentsCount = $scope.comments.length;

            });
        });
        comment.Content = "";
    }
   

}]);

app.controller("tasksCtrl", ['$scope', '$rootScope', '$http', function ($scope, $rootScope, $http) {

    //debugger;
    $scope.tasks = [];
    $scope.groups = [];
    $scope.task = {};
    $scope.taskToDelete = {};
    $scope.subjects = [];
    $scope.showAddTaskModal = false;
    $scope.showDeleteTaskModal = false;
    $scope.isTeacher = false;
    $scope.isStudent = false;
    $scope.isEdit = false;
    $scope.modalButton = '';
    $scope.filterByOption = '';
    $scope.filterOptionShow = '';
    $scope.order = 'active';
    $scope.doneOption = '';

    $scope.changeDoneFilter = function (filter) {
        switch(filter) {
            case 'done':
                $scope.doneOption = true;
                break;
            case 'todo':
                $scope.doneOption = false;
                break;
            default:
                $scope.doneOption = '';
                break;
        }
    }

    $scope.changeDeadLineFilter = function (filter) {
        if (filter == 'default')
            $scope.order = '';
        else {
            $scope.order = filter;
        }
    }

    $scope.close = function () {
        $scope.showAddTaskModal = false;
        $scope.showDeleteTaskModal = false;
    }

    $scope.checkTask = function (task) {
        task.IsDone = task.IsDone !== null ? !task.IsDone : true;

        $http({
            method: 'POST',
            url: '/Task/TaskDone',
            data: {
                TaskId: task.TaskId,
                IsDone: task.IsDone
            }
        }).then(function successCallback(responce) {
        }, function errorCallback(responce) {
            console.log("Error in checking task");
        });
    }
    
    $scope.checkIfTeacher = function () {
        $http({
            method: 'GET',
            url: '/Task/IsTeacher'
        }).then(function successCallback(response) {
            $scope.isTeacher = response.data;
        });
    }

    $scope.checkIfStudent = function () {
        $http({
            method: 'GET',
            url: '/Task/IsStudent'
        }).then(function successCallback(response) {
            $scope.isStudent = response.data;
        });
    }

    $scope.checkIfTeacher();
    $scope.checkIfStudent();

    $scope.getTasks = function () {
        $http({
            'method': 'GET',
            'url': '/Task/GetTasks'
        }).then(function successCallback(response) {
            console.log(response.data);
            $scope.tasks = response.data;
        });
    }
    $scope.getTasks();

    $scope.getGroupAndSubject = function() {
        $http({
            method: 'GET',
            url: '/Task/GetSubjectsByTeacher'
        }).then(function successCallback(response) {
            $scope.subjects = response.data;
            $http({
                method: 'GET',
                url: '/Task/GetGroupsForTeacher'
            }).then(function successCallback(response) {
                $scope.groups = response.data;
            });
        });
    }

    $scope.addTaskButton = function () {
        $scope.task = {};
        $scope.modalButton = "Add";
        $scope.showAddTaskModal = true;
        $scope.isEdit = false;        
        $scope.getGroupAndSubject();
    }

    $scope.editTaskButton = function (task) {
        $scope.getGroupAndSubject();
        $scope.modalButton = "Save"
        $scope.showAddTaskModal = true;
        $scope.isEdit = true;

        console.log(task);
        $scope.task.task_detail_id = task.TaskDetailId;
        $scope.task.task_id = task.TaskId;
        $scope.task.title = task.Title;
        $scope.task.description = task.Description;
        $scope.task.group_id = task.Group;
        $scope.task.subject_id = task.Subject; 
        //$scope.task.deadline = Date.parse(task.DeadLine);
    }

    $scope.deleteTaskButton = function (task) {
        $scope.showDeleteTaskModal = true;
        $scope.taskToDelete.id = task.TaskId;
        $scope.taskToDelete.title = task.Title;        
    }

    $scope.deleteTask = function () {
        $http({
            method: 'POST',
            url: '/Task/DeleteTask',
            data: {
                taskId: $scope.taskToDelete.id
            }
        }).then(function successCallback(responce) {
            $scope.showDeleteTaskModal = false;
            $scope.taskToDelete.id = 0;
            $scope.taskToDelete.title = '';
            $scope.getTasks();
        }, function errorCallback(responce) {
            $scope.showDeleteTaskModal = false;
        });
        
    }

    $scope.saveNewTask = function (task) {
        console.log(task.group_id)
        console.log(task);
        if (!task.description) {
            return;
        }
        $http({
            method: 'POST',
            url: '/Task/AddTask',
            data: {
                TaskId: $scope.task.task_id,
                TaskTitle: task.title,
                TaskDescription: task.description,
                GroupId: task.group_id, 
                SubjectId: task.subject_id, 
                DeadLine: task.deadline.getTime()

            }
        }).then(function successCallback(responce) {
            $scope.showAddTaskModal = false;
            $scope.task = {};
            $scope.getTasks();
        }, function errorCallback(responce) {
            $scope.showAddTaskModal = false;
        });
    }

    $scope.addFilterOption = function(filterOption, filterType) {
        $scope.filterByOption = filterOption;
        $scope.filterOptionShow = filterType + ": " + filterOption;
    }

    $scope.resetFilterOption = function () {
        $scope.filterByOption = '';
        $scope.filterOptionShow = '';
    }

    $scope.detailedTask = function (taskid) {
        window.location.href = '/Task/TaskDetail?id=' + taskid;
    }
}]);


app.controller('teachSubjCtrl', ['$scope', '$http', function ($scope, $http) {

    $scope.subjects = [];
    $scope.subject = {};
    $scope.showSubjectModal = false;
    $scope.isDelete = false;
    $scope.modalTitle = "";
    $scope.modalButton = "";

    $scope.getSubjects = function () {
        $http({
            method: 'GET',
            url: '/Manage/GetSubjects'
        }).then(function successCallback(response) {
            $scope.subjects = response.data;
            console.log($scope.subjects);
        });
    }

    $scope.getSubjects();

    $scope.close = function () {
        $scope.showSubjectModal = false;
    }

    $scope.editSubject = function (subject) {
        $scope.showSubjectModal = true;
        $scope.isDelete = false;
        $scope.modalTitle = "Edit subject";
        $scope.modalButton = "Save"

        $scope.subject.name = subject.SubjectName;
        $scope.subject.id = subject.SubjectId;
        $scope.subject.teacherId = subject.TeacherId;
    }

    $scope.addSubject = function () {
        $scope.showSubjectModal = true;
        $scope.isDelete = false;
        $scope.modalTitle = "Add subject";
        $scope.modalButton = "Add";
    }

    $scope.deleteSubject = function (subject) {
        $scope.showSubjectModal = true;
        $scope.isDelete = true;
        $scope.modalTitle = "Delete subject";
        $scope.modalButton = "Delete";

        $scope.subject.name = subject.SubjectName;
        $scope.subject.id = subject.SubjectId;
    }

    $scope.saveSubject = function (subject) {
        if ($scope.isDelete === false) {
            if (!subject.name) {
                return;
            }

            console.log('saveSubject');
            console.log(subject);

            $http({
                method: 'POST',
                url: '/Manage/SaveSubject',
                data: {
                    SubjectId: subject.id,
                    SubjectName: subject.name,
                    TeacherId: subject.teacherId
                }
            }).then(function successCallback(responce) {
                $scope.showSubjectModal = false;

                $scope.subject.name = "";
                $scope.subject.id = 0;
                $scope.subject.teacherId = "";

                $scope.getSubjects();
            }, function errorCallback(responce) {
                $scope.showSubjectModal = false;
            });
        } else {
            console.log('saveSubject - delete');
            console.log(subject);

            $http({
                method: 'POST',
                url: '/Manage/DeleteSubject',
                data: {
                    SubjectId: subject.id
                }
            }).then(function successCallback(responce) {
                $scope.showSubjectModal = false;

                $scope.subject.name = "";
                $scope.subject.id = 0;
                $scope.subject.teacherId = "";

                $scope.getSubjects();
            }, function errorCallback(responce) {
                $scope.showSubjectModal = false;
            });
        }
        

    }



}]);


app.controller("teachGroupsCtrl", ['$scope', '$http', function ($scope, $http) {

    $scope.groups = [];
    $scope.availableGroups = [];
    $scope.group = {};
    $scope.showGroupModal = false;
    $scope.isDelete = false;
    $scope.modalTitle = "";
    $scope.modalButton = "";
    
    $scope.getGroups = function () {
        $http({
            method: 'GET',
            url: '/Manage/GetGroups'
        }).then(function successCallback(response) {
            $scope.groups = response.data;
        });
    }

    $scope.getGroups();

    $scope.close = function () {
        $scope.showGroupModal = false;
    }

    $scope.addGroup = function () {
        $scope.showGroupModal = true;
        $scope.isDelete = false;
        $scope.modalButton = "Add";
        $scope.modalTitle = "Add to my groups";
        $http({
            method: 'GET',
            url: '/Manage/GetAvailableGroups'
        }).then(function successCallback(response) {
            $scope.availableGroups = response.data;
        });
    }

    $scope.deleteGroup = function (group) {
        $scope.showGroupModal = true;
        $scope.isDelete = true;
        $scope.modalButton = "Delete";
        $scope.modalTitle = "Delete from my groups";

        $scope.group.id = group.GroupId;
        $scope.group.name = group.GroupName;        
    }

    $scope.saveGroup = function (group) {       
                
        console.log(group.id);
        
        $http({
            method: 'POST',
            url: '/Manage/SaveGroup',
            data: {
                GroupId: group.id
            }
        }).then(function successCallback(response) {
            $scope.showGroupModal = false;

            $scope.group.id = 0;
            $scope.group.name = "";

            $scope.getGroups();

        }, function errorCallback(response) {
            $scope.showGroupModal = false;
        });

    }


}]);