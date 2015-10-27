angular
    .module('cecBackoffice', ['ngRoute', 'ngCQRS', 'angularforms'])
    .factory('Backoffice', function (StoreService, CQRS, AngularForms, $http) {
        var createForController = function($scope, type) {
            this.CQRS = CQRS;
            this.StoreService = StoreService;
            this.Form = AngularForms;
            this.$http = $http;

            // methods
            this.setType = function(type) {
                this._type = type;
            };

            this.setScope = function($scope) {
                this._scope = $scope;
            };

            this.sendCommand = function(command, model) {
                this.CQRS.sendCommand({
                    command: command,
                    aggregateType: this._checkAndGetType(),
                    payload: model
                });
            };

            this.create = function(model) {
                this.sendCommand('create',  model);
            };

            this.update = function(model) {
                this.sendCommand('update',  model);
            };

            this.initIndex = function() {
                var $scope = this._checkAndGetScope();
                var type = this._checkAndGetType();
                var store = this.StoreService.createForController($scope);
                console.log(type)
                // send a query to the server, requesting the view 'profile'
                // angular.CQRS will invoke your callback on the first response and on every subsequent update from the server
                // the profileData you get will be denormalized by the denormalization function you registered
                store.for(type).do(function(data) {
                    console.log(data);
                    $scope.rows = data;
                });
            };

            this.injectForm = function(injectTarget) {
                // TODO: implement convention manager
                var $scope = this._checkAndGetScope();
                var type = this._checkAndGetType();

                this.$http.get('/api/schema/' + type).then(function(response) {
                    console.log(response);
                    $scope.formFields = response.data;
                    var Form = {
                        name: 'Form', // Required. Used to set <form> id attribute
                        horizontal: false, // Set to true for Bootstrap horizontal form layout

                        fields: response.data,

                        buttons: {
                            save: {
                                label: "Save",
                                icon: "fa-refresh",
                                ngClick: "save()",
                                'class': 'btn-primary btn-sm'
                            },
                            /*
                        reset: {
                            label: "Reset",
                            icon: "fa-minus-circle",
                            ngClick: "reset()",
                            'class': 'btn-default btn-sm'
                        }
                        */
                        }
                    };

                    $scope.form = AngularForms({ scope: $scope, targetId: injectTarget, form: Form });
                    $scope.form.inject();
                });
            };

            this._init = function($scope, type) {

                this.setScope($scope);
                this.setType(type);

                this._bind();
            };

            this._bind = function() {
                var $scope = this._checkAndGetScope();
                var me = this;
                $scope.save = function() {
                    var res = {};
                    for (var i in $scope.formFields) {
                        res[i] = $scope[i];
                    }

                    me.create(res);
                };
            };

            this._checkAndGetType = function() {
                if (!this._type) {
                    throw "Type is not set yet. Please call setType(type);";
                }

                return this._type;
            };

            this._checkAndGetScope = function() {
                if (!this._scope) {
                    throw "Scope is not set yet. Please call setScope($scope);";
                }

                return this._scope;
            };

            this._init($scope, type);
            var me = this;
            CQRS.onCommand(function (cmd) {
                console.log(cmd);

                switch (cmd.command) {
                    case 'create':
                        me.$http.post('/api/' + cmd.aggregateType, cmd.payload).then(function (response) {
                            console.log(response);
                        }, function (response) {
                            console.log(response);
                        });
                }
            });
        };

        return {
            createForController: function ($scope, type) {
                return new createForController($scope, type);
            }
        };
    });