class MainClass
	def initialize (global_object)
		@go = global_object
	end
	def setup
		puts "SETUP"
	end
	def loop
		puts "LOOOOP"
	end
end

self.main_class = MainClass.new(self)

