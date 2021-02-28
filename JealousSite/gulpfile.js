const gulp = require('gulp');
const less = require('gulp-less');
const path = require('path');
const concat = require('gulp-concat');
const plumber = require('gulp-plumber');
const del = require('del');
const uglify = require('gulp-uglify');

// Compile all .less files to .css
gulp.task('less', function () {
  return gulp.src('./wwwroot/_dev/*.less')
    .pipe(plumber())
    .pipe(less({
      paths: [path.join(__dirname, 'less', 'includes')]
    }))
    .pipe(gulp.dest('./wwwroot/_dev/'));
});

// Minify and bundle CSS files
gulp.task('styles', gulp.series('less', function () {
  return gulp.src(['./wwwroot/_dev/*.css', '!./wwwroot/_dev/*.min.css'])
    //.pipe(minifyCSS())
    .pipe(concat('jealous.min.css'))
    .pipe(gulp.dest('./wwwroot/static/'));
}));

gulp.task('scripts', () => {
  return gulp.src([
    './wwwroot/_lib/*.js',
    './wwwroot/_dev/*.js',
  ])
    .pipe(uglify().on('error', function (e) { console.log(e); }))
    .pipe(concat('jealous.min.js'))
    .pipe(gulp.dest('./wwwroot/static/'));
});

// Delete all compiled and bundled files
gulp.task('clean', function () {
  return del(['./wwwroot/_dev/*.css', './wwwroot/static/*.js', './wwwroot/static/*.css']);
});

// Watch: recompile less on changes
gulp.task('watch', function () {
  gulp.watch(['./wwwroot/_dev/*.less'], gulp.series('styles'));
  gulp.watch(['./wwwroot/_dev/*.js'], gulp.series('scripts'));
});

gulp.task('default', gulp.series('clean', 'scripts', 'styles', function (done) { done(); }));








///// <binding BeforeBuild='default' Clean='clean' ProjectOpened='watch' />
//var gulp = require('gulp');
//var less = require('gulp-less');
//var path = require('path');
//var concat = require('gulp-concat');
//var plumber = require('gulp-plumber');
//var uglify = require('gulp-uglify');
//var minifyCSS = require('gulp-minify-css');
//var del = require('del');

//// Compile all .less files to .css
//gulp.task('less', function () {
//  return gulp.src('./wwwroot/_dev/*.less')
//    .pipe(plumber())
//    .pipe(less({
//      paths: [path.join(__dirname, 'less', 'includes')]
//    }))
//    .pipe(gulp.dest('./wwwroot/_dev/'));
//});

//// Delete all compiled and bundled files
//gulp.task('clean', function () {
//  return del(['./wwwroot/_dev/*.css', './wwwroot/static/*.js', './wwwroot/static/*.css']);
//});

//// Minify and bundle JS files
//gulp.task('scripts', function () {
//  return gulp.src([
//    './wwwroot/_lib/*.js',
//    './wwwroot/_dev/*.js',
//  ])
//    .pipe(uglify().on('error', function (e) { console.log(e); }))
//    .pipe(concat('jealous.min.js'))
//    .pipe(gulp.dest('./wwwroot/static/'));
//});

//// Minify and bundle CSS files
//gulp.task('styles', ['less'], function () {
//  return gulp.src(['./wwwroot/_dev/*.css', '!./wwwroot/_dev/*.min.css'])
//    .pipe(minifyCSS())
//    .pipe(concat('jealous.min.css'))
//    .pipe(gulp.dest('./wwwroot/static/'));
//});

//// Default task: full clean+build.
//gulp.task('default', ['clean', 'scripts', 'styles'], function () { });

//// Watch: recompile less on changes
//gulp.task('watch', function () {
//  gulp.watch(['./wwwroot/_dev/*.less'], ['styles']);
//  gulp.watch(['./wwwroot/_dev/*.js'], ['scripts']);
//});
