@echo off
pushd %~dp0
call bundle install --quiet
call bundle exec ruby generate.rb
popd
