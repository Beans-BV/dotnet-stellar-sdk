require 'bundler/setup'
require 'xdrgen'
require_relative 'generator/generator'
require_relative 'generator/preprocessor_check'

# Match the 4-space indentation used in C# files
Xdrgen::OutputFile.send(:remove_const, :SPACES_PER_INDENT)
Xdrgen::OutputFile.const_set(:SPACES_PER_INDENT, 4)

puts "Generating C# XDR classes..."

Dir.chdir("..")

schemes = Dir.glob("schemes/*.x")
directives = PreprocessorCheck.find_directives(schemes)
unless directives.empty?
  abort <<~MSG
    Preprocessor directives found in the XDR schemas. xdrgen has no preprocessor
    and cannot parse them; strip the feature-gated blocks by hand (see README.md,
    "Updating XDR schemas"):
      #{directives.join("\n  ")}
  MSG
end

Xdrgen::Compilation.new(
  schemes,
  output_dir: ".",
  generator: CsharpGenerator,
  namespace: "StellarDotnetSdk.Xdr",
).compile

puts "Done!"
