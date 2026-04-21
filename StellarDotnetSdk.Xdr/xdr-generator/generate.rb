require 'bundler/setup'
require 'xdrgen'
require_relative 'generator/generator'

# Match the 4-space indentation used in C# files
Xdrgen::OutputFile.send(:remove_const, :SPACES_PER_INDENT)
Xdrgen::OutputFile.const_set(:SPACES_PER_INDENT, 4)

puts "Generating C# XDR classes..."

Dir.chdir("..")

Xdrgen::Compilation.new(
  Dir.glob("schemes/*.x"),
  output_dir: ".",
  generator: CsharpGenerator,
  namespace: "StellarDotnetSdk.Xdr",
).compile

puts "Done!"
