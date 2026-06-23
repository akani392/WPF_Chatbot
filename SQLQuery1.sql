Create database chatbotdb;

use chatbotdb;

Create table Tasks(
	TaskId int identity(1,1) primary key,
	Title varchar(100) not null,
	description varchar(255) not null,
	ReminderDate datetime null,
	isCompleted BIT default 0,
	Created datetime not null
	);

select * from Tasks;