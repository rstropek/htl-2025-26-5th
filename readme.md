# Welcome to School Year 2025/26

## Exams (Leistungsfeststellungen)

* All exam dates can be found in [Webuntis](https://mese.webuntis.com/).
* Exams are practical coding exams done on a computer.
* You will have access to the content of the course's GitHub repository during exams.
* If not announced otherwise...
    * ... exams are in **EDV 10** (room 135).
    * ... exams are **without internet access** and therefore **without AI assistance**.
* You can do the exams on Windows using **Visual Studio Code**. If you want, you can use other software installed on the school's computers. However, only VSCode will have been tested by me.

In additional to the practical coding exams, you can have an optional oral exam per semester. It is a chance to mend bad marks in written exams. The oral exam can be requested by the teacher or by you.

| Grade | Min. Percentage (incl.) |
| ----- | ----------------------- |
| 1     | 89                      |
| 2     | 76                      |
| 3     | 63                      |
| 4     | 50                      |
| 5     | Less than 50 perc.      |

Note that grades are **not** given purely on calculated percentages. The calculated grade is a basis that can be adjusted by your teacher based on other factors (e.g. special achievements, special circumstances). However, deviations from calculated grades will be reasoned.

## Lightning Talks

Each student **must** give one lightning talk during the school year. Lightning talks are short presentations (maximum 8 minutes, you **must not** exceed this time limit). You can choose a topic from [this list](./lightning-talk-topics.md) or propose your own topic (make sure to get it approved by me first).

## Overall Grade

The overall grade for the course is based on your exam grades. Generally, the grade is calculated as the average of all exam grades. However, there are special considerations:

* If you demonstrate knowledge of a specific topic in a later exam that you failed in an earlier exam, the earlier exam has less weight in the overall grade.
* If you miss an exam for a valid reason (e.g. illness), the missed exam will not be counted in the overall grade.
* If you miss an exam on a topic for which we will not have another exam later in the year, you will have to demonstrate your knowledge of this topic in an oral exam.
* If your overall grade is in the borderline between two grades (between .3 and .7), your teacher can adjust your overall grade based on factors like quality of your lightning talk, quality of GitHub repository for homeworks, participation in class, etc.

## Course Prerequisites

For this course, your personal computer must meet the following requirements (latest stable versions):

* Windows, macOS, or Linux.
* Google Chrome browser (you can use other browsers, but the samples and instructions are optimized for Chrome).
* [Visual Studio Code](https://code.visualstudio.com/)
* [Node.js](https://nodejs.org/)
* [Git](https://git-scm.com/)
* [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) (only for the C# part of the course if applicable)
* Password manager like [KeePass](https://keepass.info/) or [Bitwarden](https://bitwarden.com/) to store passwords (optional but recommended)

## AI Coding Agents

AI coding agents are very useful whenever you have questions or need help while not in class. As a student, you can e.g. get [access to _GitHub Copilot_ for free](https://docs.github.com/en/education/about-github-education/github-education-for-students/apply-to-github-education-as-a-student).

However, there are a few rules regarding the use of AI coding agents in this course:

* **Do** use AI coding agents to get unstuck, to get hints, or to get code examples.
* You **must** practice writing code yourself **without** AI assistance to prepare for exams.
* During exams, you **must not** use AI coding agents.
* If you use AI coding agents, you **must** understand the code they generate and be able to explain it.
* You **must** own the code you submit e.g. for homeworks. If the AI coding agent made a mistake, this mistake is counted as yours.

## Course Repository

Create a GitHub repository where you can store all your course work.

**Once you will have created the repository, fill in [this form](https://forms.office.com/e/RrVjubivdR) to share the repository link with me.** Note that you need to sign in with your school account to access the form.

Structure the repository as follows:

```
/
├── .gitignore (store a .gitignore file here)
├── coursework (store the examples we create together in class here)
|   ├── 2025-09-11-html-dom-intro (YYYY-MM-DD-topic-name)
|   └── ...
├── homework (store your homework solutions here)
|   ├── 2025-09-18-simple-browser-game (YYYY-MM-DD-homework-name)
|   └── ...
└── projects (projects that you create outside of class for practice)
    ├── project-1
    └── ...
```

**A clean and well-structured repository is part of your grade.** Here are some good practices:

* Follow the structure above.
* Use meaningful names for folders and files.
* Copy the [.gitignore](.gitignore) file to your repository to avoid committing unnecessary files.
* Write a `README.md` file for each project explaining what the project is about.
* Write meaningful commit messages (you can use an AI coding agent to help you with this).
* Commit often (e.g. after finishing a small task).
