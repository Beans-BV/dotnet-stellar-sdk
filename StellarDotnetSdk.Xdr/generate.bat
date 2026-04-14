@echo off
pushd %~dp0xdr
call bundle install --quiet
call bundle exec ruby generate_xdr.rb "%~dp0schemes" "%~dp0." "StellarDotnetSdk.Xdr"
popd
