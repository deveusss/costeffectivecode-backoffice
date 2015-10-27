var gulp         = require('gulp'),
    sass         = require('gulp-sass'),
    concat       = require('gulp-concat'),
    browserSync  = require('browser-sync'),
    sourcemaps   = require('gulp-sourcemaps'),
    postcss      = require('gulp-postcss'),
    autoprefixer = require('autoprefixer'),
    svgmin       = require('gulp-svgmin'),
    iconfont     = require('gulp-iconfont'),
    consolidate  = require('gulp-consolidate'),
    jade         = require('gulp-jade'),
    runTimestamp = Math.round(Date.now() / 1000);

// sass
gulp.task('sass', function() {
    return gulp
        .src('src/sass/*.{sass,scss}')
        .pipe(sourcemaps.init())
        .pipe(sass({
            outputStyle: 'nested', // nested, expanded, compact, compressed
            precision: 5
        })).on('error', errorHandler)
        .pipe(postcss([
            autoprefixer({browsers: ['> 1%', 'IE 9']})
        ]))
        .pipe(sourcemaps.write('./'))
        .pipe(gulp.dest('Content'))
        .pipe(browserSync.reload({stream:true}));
});

// iconfont
gulp.task('iconfont', function(){
    return gulp.src(['src/icons/*.svg'])
        .pipe(svgmin())
        .pipe(iconfont({
            fontName: 'iconfont',
            formats: ['ttf', 'eot', 'woff', 'woff2'],
            timestamp: runTimestamp,
            normalize: true,
            fontHeight: 1001,
            fontStyle: 'normal',
            fontWeight: 'normal'
        }))
        .on('glyphs', function(glyphs, options) {
            gulp.src('src/assets/_iconfont.scss')
                .pipe(consolidate('lodash', {
                    glyphs: glyphs,
                    fontName: 'iconfont',
                    fontPath: '../fonts/',
                    className: 'icon'
                }))
                .pipe(gulp.dest('src/sass/'));
            gulp.src('src/assets/icons.jade')
                .pipe(consolidate('lodash', {
                    glyphs: glyphs
                }))
                .pipe(gulp.dest('src/jade/'));
        })
        .pipe(gulp.dest('fonts'));
});

// jade
gulp.task('jade', function(){
    gulp.src('src/jade/*.jade')
        .pipe(jade({
            pretty: true
        }))
        .pipe(gulp.dest('./'))
});

// browser-sync
gulp.task('browser-sync', function() {
    browserSync.init(null, {
        server: {
            baseDir: '.'
        }
    });
});

// bs-reload
gulp.task('bs-reload', function () {
    browserSync.reload();
});

gulp.task('build', ['sass', 'jade'], function() {
    
});

// default
gulp.task('default', ['sass', 'browser-sync'], function () {
    gulp.watch('src/icons/*.svg', ['iconfont']);
    gulp.watch('src/sass/**/*.{sass,scss}', ['sass']);
    gulp.watch('src/jade/*.jade', ['jade']);
    gulp.watch('*.html', ['bs-reload']);
});

// error handle
function errorHandler (error) {
    console.log(error.toString());
    this.emit('end');
}