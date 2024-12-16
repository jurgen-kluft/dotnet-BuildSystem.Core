enum EEnemyType
{
	Soldier = 0,
	Archer = 1,
	Knight = 2,
};


// Forward declares
class GameRoot;
class AI;
class Fonts;
class Menu;
class cars_t;
class Tracks;
class Enemy;
class car_t;
class Track;
class car_config_t;
class ModelData;

class ModelData
{
public:
	inline datafile_t<staticmesh_t> const& getStaticMesh() const { return m_StaticMesh; }
	inline array_t<datafile_t<texture_t>> const& getTextures() const { return m_Textures; }

private:
	datafile_t<staticmesh_t> m_StaticMesh;
	array_t<datafile_t<texture_t>> m_Textures;
};

class car_config_t
{
public:
	inline string_t const& getName() const { return m_Name; }
	inline f32 getWeight() const { return m_Weight; }
	inline f32 getMaxSpeed() const { return m_MaxSpeed; }
	inline f32 getAcceleration() const { return m_Acceleration; }
	inline f32 getBraking() const { return m_Braking; }
	inline f32 getCornering() const { return m_Cornering; }
	inline f32 getStability() const { return m_Stability; }
	inline f32 getTraction() const { return m_Traction; }

private:
	string_t m_Name;
	f32 m_Weight;
	f32 m_MaxSpeed;
	f32 m_Acceleration;
	f32 m_Braking;
	f32 m_Cornering;
	f32 m_Stability;
	f32 m_Traction;
};

class Track
{
public:
	inline ModelData const* getModel() const { return m_Model; }
	inline datafile_t<texture_t> const& getRoad() const { return m_Road; }

private:
	ModelData* m_Model;
	datafile_t<texture_t> m_Road;
};

class car_t
{
public:
	inline car_config_t const* getConfiguration() const { return m_Configuration; }
	inline ModelData const* getModelPath() const { return m_ModelPath; }

private:
	car_config_t* m_Configuration;
	ModelData* m_ModelPath;
};

class Enemy
{
public:
	inline EEnemyType getEnemyType() const { return m_EnemyType.get(); }
	inline f32 getSpeed() const { return m_Speed; }
	inline f32 getAggresiveness() const { return m_Aggresiveness; }

private:
	enum_t<EEnemyType, u32> m_EnemyType;
	f32 m_Speed;
	f32 m_Aggresiveness;
};

class Tracks
{
public:
	inline array_t<data_t<Track>> const& gettracks() const { return m_tracks; }

private:
	array_t<data_t<Track>> m_tracks;
};

class cars_t
{
public:
	inline array_t<car_t const *> const& getcars() const { return m_cars; }

private:
	array_t<car_t*> m_cars;
};

class Menu
{
public:
	inline string_t const& getdescr() const { return m_descr; }

private:
	string_t m_descr;
};

class Fonts
{
public:
	inline string_t const& getDescription() const { return m_Description; }
	inline datafile_t<font_t> const& getFont() const { return m_Font; }

private:
	string_t m_Description;
	datafile_t<font_t> m_Font;
};

class AI
{
public:
	inline datafile_t<curve_t> const& getReactionCurve() const { return m_ReactionCurve; }
	inline string_t const& getDescription() const { return m_Description; }
	inline array_t<Enemy const *> const& getBlueprintsAsArray() const { return m_BlueprintsAsArray; }
	inline array_t<Enemy const *> const& getBlueprintsAsList() const { return m_BlueprintsAsList; }

private:
	datafile_t<curve_t> m_ReactionCurve;
	string_t m_Description;
	array_t<Enemy*> m_BlueprintsAsArray;
	array_t<Enemy*> m_BlueprintsAsList;
};

class GameRoot
{
public:
	inline datafile_t<audio_t> const& getBootSound() const { return m_BootSound; }
	inline data_t<AI> const& getAI() const { return m_AI; }
	inline data_t<Fonts> const& getFonts() const { return m_Fonts; }
	inline data_t<Menu> const& getMenu() const { return m_Menu; }
	inline data_t<Cars> const& getCars() const { return m_Cars; }
	inline Tracks const* getTracks() const { return m_Tracks; }

private:
	datafile_t<audio_t> m_BootSound;
	data_t<AI> m_AI;
	data_t<Fonts> m_Fonts;
	data_t<Menu> m_Menu;
	data_t<Cars> m_Cars;
	Tracks* m_Tracks;
};


