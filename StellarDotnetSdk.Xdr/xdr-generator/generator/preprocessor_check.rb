# xdrgen has no C preprocessor: its parser rejects a schema that still holds an
# #ifdef block with a bare parse error that names neither the file nor the
# directive. Upstream gated blocks (e.g. CAP_0084_MUXED_CONTRACT, TEST_FEATURE)
# must be stripped by hand when copying the .x files (see README.md, "Updating
# XDR schemas"); this check reports every leftover directive by file and line.
module PreprocessorCheck
  # Any directive name, not only the conditional ones: xdrgen rejects them all.
  # A %-prefixed line (e.g. %#include) is passed through by xdrgen and never
  # matches, because it does not start with #.
  DIRECTIVE = /\A\s*#\s*[a-z]+\b/

  # Returns "path:line: text" for every preprocessor directive in the files.
  def self.find_directives(paths)
    paths.sort.flat_map do |path|
      File.readlines(path, chomp: true).each_with_index.filter_map do |line, index|
        "#{path}:#{index + 1}: #{line.strip}" if line.match?(DIRECTIVE)
      end
    end
  end
end
