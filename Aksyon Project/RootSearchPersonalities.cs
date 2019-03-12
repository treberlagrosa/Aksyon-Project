using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aksyon_Project
{
    public class RootSearchPersonalities
    {
        public class QualifierId
        {
            public int id { get; set; }
            public string description { get; set; }
        }

        public class Region
        {
            public int id { get; set; }
            public string abbrev { get; set; }
            public string description { get; set; }
        }

        public class Province
        {
            public int id { get; set; }
            public string description { get; set; }
            public int prov_region_id { get; set; }
        }

        public class Municipality
        {
            public int id { get; set; }
            public int region_id { get; set; }
            public int province_id { get; set; }
            public string description { get; set; }
        }

        public class Barangay
        {
            public int id { get; set; }
            public int region_id { get; set; }
            public int province_id { get; set; }
            public int city_id { get; set; }
            public string description { get; set; }
        }

        public class CategoryHvtslt
        {
            public int id { get; set; }
            public int hvt_slt_id { get; set; }
            public string description { get; set; }
        }

        public class ClassSuspect
        {
            public int id { get; set; }
            public int class_suspect_id { get; set; }
            public string description { get; set; }
        }

        public class DrugpersonStatus
        {
            public int id { get; set; }
            public string description { get; set; }
            public object created_at { get; set; }
            public object updated_at { get; set; }
        }

        public class RegionalOffice
        {
            public int id { get; set; }
            public string description { get; set; }
            public int seq { get; set; }
            public int cat { get; set; }
            public int pop { get; set; }
        }

        public class ProvincialOffice
        {
            public int id { get; set; }
            public int pros_id { get; set; }
            public int ppo_id { get; set; }
            public string description { get; set; }
        }

        public class PoliceStation
        {
            public int id { get; set; }
            public int pro_id { get; set; }
            public int ppo_id { get; set; }
            public string description { get; set; }
            public string uccn { get; set; }
            public string municipal_code { get; set; }
        }

        public class Personality
        {
            public int person_id { get; set; }
            public string last_name { get; set; }
            public string first_name { get; set; }
            public string middle_name { get; set; }
            public QualifierId qualifier_id { get; set; }
            public string alias { get; set; }
            public string birthday { get; set; }
            public object place_of_birth { get; set; }
            public Region region { get; set; }
            public Province province { get; set; }
            public Municipality municipality { get; set; }
            public Barangay barangay { get; set; }
            public object street_details { get; set; }
            public string gender { get; set; }
            public string occupation { get; set; }
            public object occupation_details { get; set; }
            public string related_ego { get; set; }
            public string related_ofw { get; set; }
            public object group_affiliation { get; set; }
            public object group_affiliation_pos { get; set; }
            public object citizenship { get; set; }
            public object ethnic_group { get; set; }
            public object dialect { get; set; }
            public object religion { get; set; }
            public object technical_skills { get; set; }
            public object social_media { get; set; }
            public object contact_number { get; set; }
            public string oic { get; set; }
            public string education { get; set; }
            public object remarks { get; set; }
            public object validated_by { get; set; }
            public object date_surrendered_arrested { get; set; }
            public string date_entry_watchlist { get; set; }
            public CategoryHvtslt category_hvtslt { get; set; }
            public ClassSuspect class_suspect { get; set; }
            public DrugpersonStatus drugperson_status { get; set; }
            public RegionalOffice regional_office { get; set; }
            public ProvincialOffice provincial_office { get; set; }
            public PoliceStation police_station { get; set; }
            public int encoded { get; set; }
            public string data_owner { get; set; }
            public int confidentiality { get; set; }
            public string recap { get; set; }
            public string listed { get; set; }
            public string validated { get; set; }
            public string subjected_tokhang { get; set; }
            public string image_path { get; set; }
            public List<object> hidden { get; set; }
            public int operation_count { get; set; }
        }

        public class RootObject
        {
            public List<Personality> personalities { get; set; }
        }
    }
}
