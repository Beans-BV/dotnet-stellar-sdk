@echo off
pushd %~dp0xdr
call bundle install --quiet
call bundle exec ruby generate.rb
popd
