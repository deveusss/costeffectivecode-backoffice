window.app = angular.module('shop2', ['ngResource', 'ngRoute', 'ngCQRS', 'angularforms', 'cecBackoffice'])
    .config([
        '$routeProvider', function ($routeProvider) {

            $routeProvider.when('/products/:action/:id', {
                templateUrl: 'products.html',
                controller: 'ProductsController'
            });

            $routeProvider.when('/orders', {
                templateUrl: 'orders.html',
                controller: 'OrdersController'
            });

            $routeProvider.otherwise({ redirectTo: '/products' });
        }
    ])
    .controller('OrdersController', function ($scope, $routeParams, $rootScope) {
        $rootScope.pageTitle = 'Заказы';
    })
    .controller('ProductsController', function ($scope, $routeParams, $rootScope, Backoffice) {
        $rootScope.pageTitle = 'Товары';
        var backoffice = Backoffice.createForController($scope, 'product');
        backoffice.initIndex();
        backoffice.injectForm('exampleForm');
    })
    .controller('XXProductsController', function ($scope, $routeParams, $rootScope, StoreService, CQRS, AngularForms, $http) {
        $rootScope.pageTitle = 'Товары';
        var store = StoreService.createForController($scope);

        // send a query to the server, requesting the view 'profile'
        // angular.CQRS will invoke your callback on the first response and on every subsequent update from the server
        // the profileData you get will be denormalized by the denormalization function you registered
        store.for('product').do(function (data) {
            console.log(data);
            $scope.rows = data;
        });

        $scope.pageTitle = 'Ваши продукты';

        $http.get('/api/schema/product').then(function (response) {
            console.log(response);
            $scope.formFields = response.data;
            var SampleForm = {
                name: 'SampleForm',                    // Required. Used to set <form> id attribute
                horizontal: false,                     // Set to true for Bootstrap horizontal form layout

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

            $scope.form = AngularForms({ scope: $scope, targetId: 'exampleForm', form: SampleForm });
            $scope.form.inject();
            $('#exampleForm').append('' +
                '<form method="POST" action="/api/Upload" enctype="multipart/form-data">' +
                '<div style="width: 200px; height: 200px;" id="image-preview"></div>' +
                '<input id="image-upload" type="file" name="file" />' +
                '<br /><input type="submit" />' +
                '</form>');
            
            $.uploadPreview({
                input_field: "#image-upload",   // Default: .image-upload
                preview_box: "#image-preview",  // Default: .image-preview
                label_field: "#image-label",    // Default: .image-label
                label_default: "Choose File",   // Default: Choose File
                label_selected: "Change File",  // Default: Change File
                no_label: false                 // Default: false
            });
        });



        $scope.save = function () {
            var res = {};
            for (var i in $scope.formFields) {
                res[i] = $scope[i];
            }

            CQRS.sendCommand({
                command: 'create',
                aggregateType: 'Product',
                payload: res
            });
        };

        CQRS.onCommand(function (cmd) {
            console.log(cmd);

            switch (cmd.command) {
                case 'create':
                    $http.post('/api/' + cmd.aggregateType, cmd.payload).then(function (response) {
                        console.log(response);
                    }, function (response) {
                        console.log(response);
                    });
            }
        });


    }).config(function (CQRSProvider) {

        CQRSProvider.setUrlFactory(function (viewModelName, parameters) {
            return 'api/' + viewModelName; // CQRSProvider.toUrlGETParameterString(parameters);
        });

    });
