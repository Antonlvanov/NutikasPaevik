using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutikasPaevik.Services
{
    public class LightTheme : ResourceDictionary
    {
        public LightTheme()
        {
            Add("BackgroundColor", Color.FromArgb("#F5F5F5"));
            Add("PrimaryBackgroundColor", Color.FromArgb("#ffeecc"));
            Add("FrameBackgroundColor", Color.FromArgb("#ffe6b3"));
            Add("TextColor", Color.FromArgb("#333333"));
            Add("ButtonBackgroundColor", Color.FromArgb("#ffdd99"));
            Add("ButtonTextColor", Color.FromArgb("#333333"));
            Add("AccentColor", Color.FromArgb("#FF6200EE")); 
            Add("SecondaryTextColor", Color.FromArgb("#666666"));
            Add("TertiaryTextColor", Color.FromArgb("#000000"));

            //planner
            Add("EventColor", Color.FromArgb("#87CEEB"));
            Add("TaskColor", Color.FromArgb("#bfbfbf"));

            //shell
            Add("HomeIcon", "home_black.png");
            Add("DiaryIcon", "diary_black.png");
            Add("ScheduleIcon", "schedule_black.png");
            Add("AccountIcon", "account_black.png");
            Add("SettingsIcon", "settings_black.png");
            Add("LogoutIcon", "logout_black.png");

            //diary
            Add("ApplyIcon", "apply_black.png");
            Add("AddIcon", "add_black.png");
            Add("EditIcon", "edit_black.png");
            Add("DeleteIcon", "delete_black.png");
            Add("MenuIcon", "menu_black.png");
            Add("BackIcon", "back_black.png");

            //calender
            Add("TodayColor", Color.FromArgb("#333333"));

            Add("EventIcon", "event_black");
            Add("TaskIcon", "task_black");

            Add("NoteIcon", "note_black.png");
        }
    }

    public class DarkTheme : ResourceDictionary
    {
        public DarkTheme()
        {
            Add("BackgroundColor", Color.FromArgb("#212121"));
            Add("PrimaryBackgroundColor", Color.FromArgb("#424242"));
            Add("FrameBackgroundColor", Color.FromArgb("#333333"));
            Add("TextColor", Color.FromArgb("#FFFFFF"));
            Add("ButtonBackgroundColor", Color.FromArgb("#757575"));
            Add("ButtonTextColor", Color.FromArgb("#FFFFFF"));
            Add("AccentColor", Color.FromArgb("#FFBB86FC"));
            Add("SecondaryTextColor", Color.FromArgb("#CCCCCC"));
            Add("TertiaryTextColor", Color.FromArgb("#000000"));

            //planner
            Add("EventColor", Color.FromArgb("#87CEEB"));
            Add("TaskColor", Color.FromArgb("#d9d9d9"));

            //shell
            Add("HomeIcon", "home_white.png");
            Add("DiaryIcon", "diary_white.png");
            Add("ScheduleIcon", "schedule_white.png");
            Add("AccountIcon", "account_white.png");
            Add("SettingsIcon", "settings_white.png");
            Add("LogoutIcon", "logout_white.png");

            //diary
            Add("ApplyIcon", "apply_white.png");
            Add("AddIcon", "add_white.png");
            Add("EditIcon", "edit_white.png");
            Add("DeleteIcon", "delete_white.png");
            Add("MenuIcon", "menu_white.png");
            Add("BackIcon", "back_white.png");

            //calender
            Add("TodayColor", Color.FromArgb("#333333"));

            Add("EventIcon", "event_black");
            Add("TaskIcon", "task_black");
        }
    }

    public class CustomTheme : ResourceDictionary
    {
        public CustomTheme()
        {
            Add("BackgroundColor", Color.FromArgb("#E6FFE6"));
            Add("PrimaryBackgroundColor", Color.FromArgb("#CCFFCC"));
            Add("FrameBackgroundColor", Color.FromArgb("#99FF99"));
            Add("TextColor", Color.FromArgb("#006400"));
            Add("ButtonBackgroundColor", Color.FromArgb("#66CC66"));
            Add("ButtonTextColor", Color.FromArgb("#006400"));
            Add("AccentColor", Color.FromArgb("#FF00FF00"));
            Add("SecondaryTextColor", Color.FromArgb("#339933")); 
            Add("TertiaryTextColor", Color.FromArgb("#000000"));

            //planner
            Add("EventColor", Color.FromArgb("#87CEEB")); 
            Add("TaskColor", Color.FromArgb("#bfbfbf"));

            //shell
            Add("HomeIcon", "home_black.png");
            Add("DiaryIcon", "diary_black.png");
            Add("ScheduleIcon", "schedule_black.png");
            Add("AccountIcon", "account_black.png");
            Add("SettingsIcon", "settings_black.png");
            Add("LogoutIcon", "logout_black.png");

            //diary
            Add("ApplyIcon", "apply_black.png");
            Add("AddIcon", "add_black.png");
            Add("EditIcon", "edit_black.png");
            Add("DeleteIcon", "delete_black.png");
            Add("MenuIcon", "menu_black.png");
            Add("BackIcon", "back_black.png");

            //calender
            Add("TodayColor", Color.FromArgb("#333333"));

            Add("EventIcon", "event_black");
            Add("TaskIcon", "task_black");
        }
    }
}
